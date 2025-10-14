# Inmate Search Web Application

A web application that automates the process of searching for inmates in Tarrant County and creating accounts on AccessCorrections.com.

## Features

- **Modern Web UI**: Clean, responsive interface built with Bootstrap 5
- **Inmate Search**: Automatically searches Tarrant County inmate database
- **Account Creation**: Automates the AccessCorrections.com signup process
- **Real-time Progress**: Visual feedback during the automation process
- **Form Validation**: Client-side and server-side validation

## How It Works

1. **Search Inmate**: Searches Tarrant County inmate database using the provided last name
2. **Select First Result**: Automatically selects the first inmate found in the search results
3. **Create AccessCorrections Account**: Attempts to create a new account on AccessCorrections.com
4. **Complete Signup Process**: Fills out all required forms including personal info, billing address, and password

## Prerequisites

- .NET 8.0 SDK
- Microsoft Playwright (will be installed automatically)

## Installation

1. Clone or download the project files
2. Navigate to the project directory
3. Install Playwright browsers:
   ```bash
   dotnet run -- --install-playwright
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Usage

1. Open your web browser and navigate to `https://localhost:5001` (or the URL shown in the console)
2. Fill in the required information:
   - **First Name**: Your first name
   - **Last Name**: The last name to search for in the inmate database
   - **Email/Username**: Your email address for the AccessCorrections account
   - **Address**: Your full address for billing information
3. Click "Start Search" to begin the automation process
4. A browser window will open automatically to perform the search and account creation
5. Monitor the progress in the browser window

## Important Notes

- The application runs the browser in non-headless mode so you can monitor the process
- The automation will attempt to create an account on AccessCorrections.com with the provided information
- Default values are used for some fields (phone, middle name, date of birth, etc.)
- The process may take several minutes to complete

## Project Structure

```
InmateSearchWebApp/
├── Controllers/
│   └── HomeController.cs          # Main controller handling form submission
├── Models/
│   ├── InmateSearchRequest.cs     # Model for form data
│   └── AccessCorrectionsOptions.cs # Configuration for AccessCorrections
├── Services/
│   ├── InmateSearchService.cs     # Main service orchestrating the process
│   └── AccessCorrectionsBot.cs    # Automation logic for AccessCorrections
├── Views/
│   ├── Home/
│   │   └── Index.cshtml           # Main form view
│   └── Shared/
│       └── _Layout.cshtml         # Layout template
├── wwwroot/
│   ├── css/
│   │   └── site.css              # Custom styling
│   └── js/
│       └── site.js               # JavaScript functionality
├── Program.cs                     # Application entry point
└── InmateSearchWebApp.csproj     # Project file
```

## Dependencies

- Microsoft.Playwright (1.40.0)
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation (8.0.0)
- Bootstrap 5 (via CDN)
- jQuery (via CDN)
- Font Awesome (via CDN)

## Troubleshooting

- Ensure you have .NET 8.0 SDK installed
- Make sure Playwright browsers are installed
- Check that all required fields are filled before submitting
- Monitor the browser window for any errors during automation