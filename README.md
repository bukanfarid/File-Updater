<br/>
<p align="center">
  <h3 align="center">File Updater</h3>

  <p align="center"> 
    <br/>
    <br/>
    <a href="https://github.com/bukanfarid/File-Updater"><strong>Explore the docs »</strong></a>
    <br/>
    <br/>
    <a href="https://github.com/bukanfarid/File-Updater">View Demo</a>
    .
    <a href="https://github.com/bukanfarid/File-Updater/issues">Report Bug</a>
    .
    <a href="https://github.com/bukanfarid/File-Updater/issues">Request Feature</a>
  </p>
</p>

![Downloads](https://img.shields.io/github/downloads/bukanfarid/File-Updater/total) ![Contributors](https://img.shields.io/github/contributors/bukanfarid/File-Updater?color=dark-green) ![Issues](https://img.shields.io/github/issues/bukanfarid/File-Updater) ![License](https://img.shields.io/github/license/bukanfarid/File-Updater) 

## About The Project

This project shows you how to update files on client local computer, while your project is desktop based and you cannot access their computer directly .

## Built With

Project ini didukung oleh Newtonsoft.Json
Dibuat dengan menggunakan VB .NET

## Getting Started

Silahkan build project ini hingga menghasilkan file "File Updater.dll", lalu import ke project anda.

Buat instance dari class FileUpdater pada form utama agar  pengecekan selalu berjalan di awal form dibuka.
Sebagai alternatif, anda bisa memanggil fungsi update dalam event timer

### Prerequisites

Buat file JSON dengan contoh format seperti berikut : 
 
>"aplikasi":{
>	"downloadPath":"https://server.com/aplikasi.zip", 
>	"versi":"1.0.3.6"
>}  

## Usage

>Dim filename As String = AppDomain.CurrentDomain.FriendlyName
>Dim filepath As String = Application.StartupPath

>Dim fileUpdater as New FileUpdater("https://server.com/tempatJson/","fileReferensi.json", true)
>Dim hasil as String = await fileUpdater.checkThenUpdate(filename, filepath, "aplikasi")

>if hasil<>"Updated" then messagebox.show(hasil, "Terjadi Kesalahan")

### Creating A Pull Request



## Authors

* **BukanFarid** - ** - [BukanFarid](http://aititeru.com/) - **

## Acknowledgements

* [ShaanCoding](https://github.com/ShaanCoding/)
* [Othneil Drew](https://github.com/othneildrew/Best-README-Template)
* [ImgShields](https://shields.io/)
