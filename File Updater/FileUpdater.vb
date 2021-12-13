Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq

Public Class FileUpdater
    Private url As String
    Private jsonName As String
    Private message As String = ""
    Public appJson As JObject = Nothing

    ''' <summary>
    ''' Fitur ini untuk override url dan nama file json dari library Laritta, kemudian langsung menarik data JSON
    ''' </summary>
    ''' <param name="baseUrl">URL tempat file json diletakkan. Utamakan URL menggunakan HTTPS</param>
    ''' <param name="jsonFileName">Nama file json yang akan diakses</param>
    ''' <param name="autoPull">Parameter apakah akan langsung menarik JSON di server atau tidak</param>
    Sub New(ByVal baseUrl As String, ByVal jsonFileName As String, Optional ByVal autoPull As Boolean = True)
        url = baseUrl
        jsonName = jsonFileName

        If autoPull Then pullJSON()
    End Sub

    ''' <summary>
    ''' Fitur untuk mengambil data JSON. Gunakan fitur ini apabila dalam 1 file JSON terdapat list untuk banyak file. Sehingga penarikan JSON tidak perlu dilakukan berkali-kali untuk banyak file. Result True jika JSON berhasil di load, dan FALSE apabila gagal.
    ''' </summary>
    Private Function pullJSON() As Boolean
        Dim request As HttpWebRequest
        Dim response As HttpWebResponse = Nothing
        Dim reader As StreamReader

        If url.ToLower.Contains("https") Then ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
        Try
            Dim randomKey As String = DateTime.Now.ToString("yyMMddHHmmss")
            request = DirectCast(WebRequest.Create(url & jsonName & "?" & randomKey), HttpWebRequest)
            response = DirectCast(request.GetResponse(), HttpWebResponse)
            reader = New StreamReader(response.GetResponseStream())
            appJson = JObject.Parse(reader.ReadToEnd)
        Catch ex As Exception
            appJson = Nothing
        End Try
        Return appJson IsNot Nothing
    End Function

    ''' <summary>
    ''' Fitur ini digunakan untuk mengupdate setelah JSON berhasil di load. Apabila file JSON ternyata belum di load, maka proses akan mencoba membaca ulang json, lalu berhenti di awal jika gagal. Return string kosong jika tidak ada error, return "Updated" Jika terjadi update, return string lain jika terjadi error. Object kedua yg di return adalah object yg akan digunakan
    ''' </summary>
    ''' <param name="filename">Nama File yang akan dibandingkan versinya</param>
    ''' <param name="filePath">Lokasi File</param>
    ''' <param name="key">Kode unik aplikasi</param> 
    Public Async Function checkThenUpdate(ByVal filename As String, ByVal filePath As String, ByVal key As String) As Task(Of String)

        'Kalau appjson tidak terload, maka cancel
        If appJson Is Nothing Then
            'Coba load json ulang. Kalau tidak bisa, return gagal
            If Not pullJSON() Then
                Return "Terjadi kesalahan saat mengakses server"
            End If
        End If

        Dim result As String = ""

        Try
            'Nilai default wajib update, in case file not exist
            Dim shouldUpdate As Boolean = True

            'Jika file ada, maka cek perlu update apa engga
            If File.Exists(filePath & "\" & filename) Then
                'Dapatkan versi dari file. Running file sudah di rename, sehingga selalu dibandingkan dengan versi dari file exe.
                Dim versionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(filePath & "\" & filename)
                Dim version As String = versionInfo.FileVersion
                Dim currentVersion() As String = version.Split(CChar("."))

                'Ambil nilai json berdasarkan key
                Dim detail As JObject = appJson(key)

                'Hapus jika file .zip ada (file update sebelumnya)
                If File.Exists(filePath & "\" & key & ".zip") Then File.Delete(filePath & "\" & key & ".zip")

                'Bandingkan versi, apakah perlu update atau tidak

                Dim newVersion() As String = appJson(key)("versi").ToString().Split(CChar("."))

                ' cek apakah di server lebih baru 
                For i As Integer = 0 To currentVersion.Length - 1
                    If CInt(newVersion(i)) > CInt(currentVersion(i)) Then
                        shouldUpdate = True
                        Exit For
                    ElseIf CInt(newVersion(i)) < CInt(currentVersion(i)) Then
                        shouldUpdate = False
                        Exit For
                    End If
                Next
            End If

            Dim downloadPath As String = url & appJson(key)("downloadPath").ToString

            If shouldUpdate Then
                result = Await downloadUpdate(downloadPath, key, filePath & "\" & filename)

                If result = "" Then
                    If updateApplication(filePath & "\" & key & ".zip") = "" Then result = "Updated"
                End If

            End If
        Catch ex As Exception
            result = "Error Check Then Update : " & ex.Message.ToString
        End Try

        Return result
    End Function

    ''' <summary>
    ''' Fungsi ini dijalankan apabila sub checkThenUpdate memutuskan untuk melakukan update. Return string kosong kalau tidak ada error
    ''' </summary>
    ''' <param name="path">Url ke file yang akan di download</param>
    ''' <param name="key">Key dari aplikasi dalam json. Key ini berada di dalam key apps</param>
    ''' <param name="filename">Nama file aplikasi berjalan. Akan di rename apabila ada update</param>
    ''' <returns></returns>
    Private Async Function downloadUpdate(ByVal path As String, ByVal key As String, ByVal filename As String) As Task(Of String)
        Dim result As String = ""

        'ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
        Dim randomKey As String = DateTime.Now.ToString("yyMMddHHmmss")

        Try
            Using client As New WebClient
                client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)")
                client.CachePolicy = New Cache.RequestCachePolicy(Cache.RequestCacheLevel.NoCacheNoStore)

                Await client.DownloadFileTaskAsync(New Uri(path & "?" & randomKey), key & ".zip")
            End Using
        Catch ex As Exception
            Dim pesanError As String = ex.Message.ToString
            'Bypass error ini karena file tidak dapat diakses akibat masih proses download
            If pesanError.ToLower.Contains("the process cannot access the file") Then
                result = "-"
            ElseIf pesanError.ToLower.Contains("an existing connection was forcibly closed") Then
                result = "-"
            Else
                result = "Error Download Update : " & pesanError
            End If

        End Try

        Return result
    End Function


    ''' <summary>
    ''' Method ini berfungsi untuk mengekstrak file zip yang sudah di download. Apabila terjadi kesalahan maka akan mereturn string dengan isi pesan. Apabila lancar maka akan me return string kosong.
    ''' </summary>
    ''' <param name="zipName">Nama file .zip yang akan di ekstrak</param> 
    ''' <returns></returns>
    Private Function updateApplication(ByVal zipName As String) As String
        Dim result As String = ""

        If File.Exists(zipName) Then
            Using arch As ZipArchive = ZipFile.OpenRead(zipName)
                For Each entry As ZipArchiveEntry In arch.Entries
                    Try
                        '== ORIGINAL FILE
                        Dim fullpath As String = Path.Combine(Application.StartupPath, entry.FullName)
                        Dim fnames() As String = fullpath.Split(IO.Path.DirectorySeparatorChar)
                        Dim fname As String = fnames(fnames.Length - 1)

                        'kalau file backup lama ada 
                        If File.Exists(fullpath & ".bak") Then
                            Try
                                'Coba hapus file backup
                                File.Delete(fullpath & ".bak")
                            Catch ex As Exception
                                'Kalau gagal, berarti file backup masih running, maka hapus file aslinya
                                File.Delete(fullpath)
                            End Try
                        End If

                        'Kalau file ada, maka buat file backup
                        If File.Exists(fullpath) Then
                            My.Computer.FileSystem.RenameFile(fname, fname & ".bak")
                        End If

                        'Kalau tidak ada folder, maka dibuat
                        If String.IsNullOrEmpty(entry.Name) Then
                            Directory.CreateDirectory(Path.GetDirectoryName(fullpath))
                        End If

                        entry.ExtractToFile(fullpath, True)
                    Catch ex As Exception
                        result = "Error Update Application " & ex.Message.ToString
                    End Try
                Next
            End Using
            File.Delete(zipName)
        End If

        Return result
    End Function
End Class
