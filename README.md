# File-Updater
This project shows you how to update files on client local computer, while your project is desktop based and you cannot access their computer directly .

[V1.0.0]
Project ini didukung oleh Newtonsoft.Json.
Silahkan build project ini hingga menghasilkan file "File Updater.dll", lalu import ke project anda.

Buat instance dari class FileUpdater pada form utama agar  pengecekan selalu berjalan.

#Contoh Penggunaan
Dim filename As String = AppDomain.CurrentDomain.FriendlyName
Dim filepath As String = Application.StartupPath

Dim fileUpdater as New FileUpdater("https://server.com/tempatJson/","fileReferensi.json", true)
Dim hasil as String = await fileUpdater.checkThenUpdate(filename, filepath, "aplikasi")

if hasil<>"Updated" then messagebox.show(hasil, "Terjadi Kesalahan")


#Contoh Struktur JSON

"aplikasi":{
	"downloadPath":"https://server.com/aplikasi.zip", 
	"versi":"1.0.3.6"
}

#Catatan
Pastikan file yang diupload ke server di compress dalam bentuk ZIP.
Anda bisa menambahkan banyak file yang ingin anda update dalam 1 file ZIP

