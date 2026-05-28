# Smart Event Management and Ticketing System

A modern web-based event management platform built with ASP.NET Core MVC and .NET 8. This system helps organizations manage events efficiently while allowing users to discover, book, and review events through a seamless digital experience.

## Features

* Event discovery with smart search filters
* Member and guest role management
* Online ticket booking system
* QR code-based ticket generation
* Community reviews and feedback
* Personalized user experience
* Secure authentication and authorization
* Real-time event availability management

## Tech Stack

### Backend

* ASP.NET Core MVC (.NET 8)
* Entity Framework Core
* MySQL Database

### Packages & Libraries

* Pomelo.EntityFrameworkCore.MySql
* BCrypt.Net-Next
* QRCoder
* System.Drawing.Common

## Project Structure

```bash
Event_Manag_Booking/
│
├── Controllers/
├── Models/
├── Views/
├── Data/
├── wwwroot/
├── Services/
├── appsettings.json
└── Program.cs
```

## Installation & Setup

### Prerequisites

* .NET 8 SDK
* MySQL Server
* Visual Studio 2022 or VS Code

### Clone the Repository

```bash
git clone https://github.com/dayastant/Event_Manag_Booking.git
```

### Navigate to Project Directory

```bash
cd Event_Manag_Booking
```

### Configure Database

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;database=eventdb;user=root;password=yourpassword"
}
```

### Run Migrations

```bash
dotnet ef database update
```

### Run the Application

```bash
dotnet run
```

## Key Functionalities

### Guest Users

* View available events
* Read event reviews
* Search events by category, date, and location

### Registered Members

* Book tickets online
* Submit event reviews
* Access personalized event experiences

### Security

* Password hashing using BCrypt
* Secure authentication system
* Role-based access management

## Learning Outcomes

This project helped improve knowledge in:

* ASP.NET Core MVC architecture
* Database management with Entity Framework Core
* Authentication and security implementation
* System analysis and design
* User experience planning

## Future Improvements

* Online payment gateway integration
* Admin dashboard and analytics
* Email notifications
* Event recommendation system
* Mobile responsive optimization

## GitHub Repository

Repository Link:
https://github.com/dayastant/Event_Manag_Booking

## Author

Developed by Daya

Special thanks to Neerayan for the continuous support and encouragement throughout this project.
