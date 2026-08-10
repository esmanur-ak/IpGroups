# IP Groups Management System

IP Groups Management System, kurum içerisindeki **IP gruplarının, IP adreslerinin, binaların, birimlerin ve personellerin** merkezi bir sistem üzerinden yönetilmesini sağlayan web tabanlı bir uygulamadır.

Proje, kullanıcıların yetkilerine göre farklı işlemler gerçekleştirebildiği bir yönetim sistemi olarak geliştirilmiştir. IP gruplarının oluşturulması, düzenlenmesi ve silinmesinin yanı sıra IP adreslerinin yönetimi ve ilgili kurum bilgilerinin birbirleriyle ilişkilendirilmesi amaçlanmıştır.

 <img width="1917" height="912" alt="login" src="https://github.com/user-attachments/assets/cba778e5-4bfa-4568-b998-4c61178bbd93" />


## 🚀 Projenin Amacı

Bu projenin temel amacı, kurum içerisinde kullanılan IP adreslerinin ve bunlarla ilişkili bilgilerin düzenli ve merkezi bir şekilde yönetilmesini sağlamaktır.

Uygulama sayesinde:

* IP grupları yönetilebilir.
* IP adresleri takip edilebilir.
* Bina ve birim bilgileri yönetilebilir.
* Personel kayıtları oluşturulabilir ve yönetilebilir.
* Kullanıcı rollerine göre yetkilendirme yapılabilir.
* Kullanıcıların erişebileceği işlemler izinler üzerinden kontrol edilebilir.
* Veriler ilişkisel bir veritabanında güvenli şekilde saklanabilir.

## 📊 Kontrol Paneli

Anasayfada toplam personel, bina, birim, IP grubu sayıları ile IP atama oranları canlı olarak görüntülenir. IP gruplarının doluluk oranları ve atama durumları grafiklerle özetlenir.

 <img width="1892" height="905" alt="dashboard" src="https://github.com/user-attachments/assets/57a8ee10-0df6-4af7-86f2-db6dc4c48a5e" />


## ✨ Özellikler

### IP Grup Yönetimi

* IP gruplarını listeleme
* Yeni IP grubu oluşturma
* IP grubu bilgilerini güncelleme
* IP gruplarını silme
* IP grubuna ait IP adreslerini görüntüleme
* IP adreslerinin veritabanı ile senkronizasyonunu sağlama

### IP Adres Yönetimi

IP grupları içerisinde bulunan IP adreslerinin takip edilmesi ve yönetilmesi sağlanmaktadır.

Sistem, IP grupları görüntülenirken IP adreslerinin güncel durumunu kontrol ederek gerekli senkronizasyon işlemlerini gerçekleştirecek şekilde tasarlanmıştır.

### Kurum Bilgileri Yönetimi

Uygulamada IP adresleriyle ilişkili kurumsal yapıların yönetilebilmesi için:

* Bina
* Birim
* Personel

gibi farklı veri modelleri bulunmaktadır.

### Kullanıcı ve Yetkilendirme Sistemi

Uygulamada **ASP.NET Core Identity** kullanılarak kullanıcı yönetimi ve yetkilendirme mekanizması oluşturulmuştur.

Kullanıcıların gerçekleştirebileceği işlemler belirli izinler üzerinden kontrol edilmektedir.

Örneğin:

* Görüntüleme
* Oluşturma
* Güncelleme
* Silme

gibi işlemler için farklı yetkilendirme kontrolleri uygulanmaktadır. Her kullanıcıya, Bina, Birim, IP, Personel ve Rol & Yetki modülleri için ayrı ayrı izin ataması yapılabilir.

<img width="1902" height="910" alt="kullanici-izinleri" src="https://github.com/user-attachments/assets/17396830-6892-41c5-9ad8-013febd93d6b" />


## 🛠️ Kullanılan Teknolojiler

| Teknoloji               | Kullanım Alanı              |
| ----------------------- | --------------------------- |
| C#                      | Uygulama geliştirme         |
| ASP.NET Core MVC        | Web uygulaması altyapısı    |
| Entity Framework Core   | ORM ve veritabanı işlemleri |
| PostgreSQL              | Veritabanı                  |
| ASP.NET Core Identity   | Kullanıcı ve rol yönetimi   |
| Razor Views             | Kullanıcı arayüzü           |
| HTML / CSS / JavaScript | Frontend                    |
| Dependency Injection    | Servis yönetimi             |

Projenin `.csproj` dosyasında ASP.NET Core Web SDK, Entity Framework Core Tools, PostgreSQL için Npgsql Entity Framework Core provider ve ASP.NET Core Identity Entity Framework Core paketleri kullanılmaktadır.

## 🏗️ Proje Mimarisi

Proje, sorumlulukların farklı katmanlara ayrıldığı bir yapı kullanmaktadır.

```text
IpGroups
│
├── Constants
├── Controllers
│   ├── AccountController
│   ├── BinaController
│   ├── BirimController
│   ├── HomeController
│   ├── IpController
│   ├── PersonController
│   └── RoleController
│
├── Data
├── Extensions
├── Migrations
├── Models
│   ├── Entities
│   ├── ViewModel
│   └── ViewModels
│
├── Services
│   ├── Abstract
│   └── Concrete
│
├── Views
├── wwwroot
├── Program.cs
└── appsettings.json
```

Controller katmanında IP, bina, birim, personel, hesap ve rol yönetimi gibi farklı sorumluluklar ayrıştırılmıştır.

Servis katmanı ise `Abstract` ve `Concrete` olarak ayrılarak uygulama içerisindeki iş mantığının controller'lardan ayrılmasını sağlamaktadır.

Model yapısında entity ve ViewModel sınıfları ayrı tutulmuştur.

## 🔐 Güvenlik ve Yetkilendirme

Uygulamada kullanıcıların gerçekleştirebileceği işlemler **policy-based authorization** yaklaşımıyla kontrol edilmektedir.

Örneğin IP yönetiminde görüntüleme, oluşturma, düzenleme ve silme işlemleri için ayrı izin kontrolleri bulunmaktadır.

Ayrıca form işlemlerinde **Anti-Forgery Token** kullanılarak CSRF saldırılarına karşı koruma uygulanmıştır.

## 🗄️ Veritabanı

Proje veritabanı işlemleri için **PostgreSQL** ve **Entity Framework Core** kullanmaktadır.

Entity Framework Core migrations yapısı sayesinde veritabanı şemasındaki değişikliklerin kontrollü bir şekilde yönetilmesi amaçlanmıştır.

## ⚙️ Kurulum

### Gereksinimler

Projeyi çalıştırmak için aşağıdakilerin sisteminizde bulunması gerekir:

* .NET 10 SDK
* PostgreSQL
* Git
* Visual Studio / Visual Studio Code

### Projeyi Klonlama

```bash
git clone https://github.com/esmanur-ak/IpGroups.git
cd IpGroups
```

### Veritabanı Yapılandırması

`appsettings.json` içerisinde PostgreSQL bağlantı bilgilerinin yapılandırılması gerekir.

Örnek:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=IpGroups;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Migration

Gerekli migration işlemlerinden sonra uygulama çalıştırılabilir.

```bash
dotnet restore
dotnet ef database update
dotnet run
```

## 📚 Öğrenilen Konular

Bu proje geliştirilirken aşağıdaki konularda pratik kazanılmıştır:

* ASP.NET Core MVC
* C# ile web uygulaması geliştirme
* Entity Framework Core
* PostgreSQL
* Database Migration
* Dependency Injection
* Service Layer kullanımı
* ViewModel kullanımı
* ASP.NET Core Identity
* Role-Based / Policy-Based Authorization
* CRUD işlemleri
* AJAX tabanlı işlemler
* Katmanlı ve sürdürülebilir proje yapısı

## 👥 Proje Ekibi

Bu proje **iki kişilik bir ekip çalışması** olarak geliştirilmiştir.

* **Esmanur Ak** — [GitHub](https://github.com/esmanur-ak)
* **Halime Bulut** - [GitHub](https://github.com/Halime-blt)

Projenin geliştirme sürecinde analiz, tasarım, backend geliştirme, veritabanı işlemleri ve test aşamalarında birlikte çalışılmıştır.

## 📌 Geliştirme Notu

Bu proje, gerçek bir kurum içerisindeki IP ve kurumsal bilgi yönetimi süreçlerinin dijitalleştirilmesine yönelik bir çalışma olarak geliştirilmiştir.

Proje geliştikçe yeni özelliklerin eklenmesi ve mevcut yapıların iyileştirilmesi planlanmaktadır.

## 📄 Lisans

Bu proje eğitim ve geliştirme amacıyla oluşturulmuştur.
