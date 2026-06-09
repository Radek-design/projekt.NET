# projekt.NET - Platforma społecznościowa dla graczy

Platforma webowa stworzona w architekturze **ASP.NET Core MVC**, służąca do zarządzania osobistą biblioteką gier wideo, wymieniania się opiniami oraz dyskusji na forum.

## 🛠 Technologie i wykorzystane biblioteki

Projekt opiera się na platformie **.NET 8.0** i wykorzystuje następujące pakiety:

* **ASP.NET Core MVC** - główny szkielet aplikacji (wbudowany w .NET 8).
* **Microsoft.EntityFrameworkCore (v8.0.0)** - system ORM do komunikacji z bazą danych.
* **Pomelo.EntityFrameworkCore.MySql (v8.0.0)** - provider bazy danych **MySQL** dla Entity Framework Core.
* **Microsoft.AspNetCore.Identity.EntityFrameworkCore (v8.0.0)** - mechanizm uwierzytelniania, autoryzacji i zarządzania rolami użytkowników.
* **QuestPDF (v2026.5.0)** - zaawansowana biblioteka z interfejsem Fluent API do generowania raportów PDF ze statystykami gracza.
* **AutoMapper.Extensions.Microsoft.DependencyInjection (v12.0.1)** - biblioteka do automatycznego mapowania modeli na obiekty DTO (Data Transfer Objects).
* **Bootstrap & jQuery** - technologie front-endowe zapewniające responsywność i walidację po stronie klienta (bez przeładowywania strony).

## ⚙️ Wymagania wstępne

Aby uruchomić projekt lokalnie, upewnij się, że posiadasz:
1. Zainstalowany **.NET 8.0 SDK**.
2. Serwer bazy danych **MySQL** (np. XAMPP, MySQL Server) lub dostęp do chmurowej bazy MySQL.
3. Podstawową znajomość wiersza poleceń / terminala.

## 🚀 Instrukcja instalacji i konfiguracji

### 1. Pobranie projektu
Sklonuj repozytorium na swój dysk lokalny lub wypakuj plik ZIP do wybranego folderu, a następnie przejdź do katalogu głównego projektu.
W pliku Program.cs aplikacja jest rygorystycznie skonfigurowana do używania bazy danych MySQL (przy użyciu UseMySql).
Musisz zaktualizować ciąg połączeniowy w pliku appsettings.json, aby wskazywał na Twój serwer bazy danych.

### 2. Konfiguracja bazy danych (MySQL)
Otwórz appsettings.json i podmień sekcję ConnectionStrings na poprawny format MySQL (ponieważ aktualnie znajduje się tam format lokalny MSSQL):
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=GameDB;User=root;Password=twoje_haslo;"
}

### 3. Konfiguracja kluczy zewnętrznych API
Aplikacja wykorzystuje AI do generowania aktualności w HomeController oraz RAWG Api do pobierania danych o grach. Aby te funkcje działały, musisz dodać swój klucz API dla Google Gemini oraz klucz API dla Rawg.
W pliku appsettings.json dodaj nowe sekcje AiSettings oraz RawgApi:
"AiSettings": {\
    "GeminiApiKey": "Twoje_API"\
},\
"RawgApi": {\
    "ApiKey": "Twoje_Api"\
},
### 4. Migracja bazy danych
Przed pierwszym uruchomieniem aplikacji musisz wygenerować strukturę bazy danych. Otwórz terminal w folderze projektu (tam, gdzie znajduje się plik .csproj) i wykonaj:

# Instalacja narzędzi EF Core (jeśli nie posiadasz)
dotnet tool install --global dotnet-ef\
Add-Migration 'NazwaMigracji'\
Update-Database

## 5. Uruchomienie aplikacji
Gdy baza jest gotowa, możesz uruchomić projekt:

### 🔐 Uwagi dotyczące ról i uprawnień
Przy pierwszym uruchomieniu projektu, aplikacja (w pliku Program.cs) automatycznie utworzy w bazie dwie podstawowe role: User oraz Moderator.
Każdy nowo zarejestrowany użytkownik automatycznie otrzymuje rolę User.
Aby zarządzać systemem (dodawać gry, zarządzać forum), potrzebujesz roli Moderator. W środowisku deweloperskim musisz ręcznie nadać swojemu kontu tę rolę bezpośrednio z poziomu bazy danych (np. poprzez przypisanie UserId Twojego nowo założonego konta do RoleId dla roli Moderator w tabeli AspNetUserRoles).\
Jedne z takich środowisk to MySql Workbench \
Tutaj jest cała dokumentacja tego środowiska https://dev.mysql.com/doc/workbench/en/
