# DillonColeman-PaySkyAssessment

## Overview
DillonColeman-PaySkyAssessment is a .NET 8 application designed to meet the requirements of the PaySky Assessment. The application provides functionality for two user types: Employers and Applicants, each with specific roles and functions. The system offers features such as user registration, login, vacancy management, application submission, and more. This README provides instructions for setting up and running the application.

## System Requirements
To successfully run the PaySky Assessment application, ensure that you have the following components in place:

- .NET 8 runtime or SDK installed
- Microsoft SQL Server for the database
- Visual Studio (or any compatible IDE) for development

## Setup Instructions

Follow these steps to set up and run the application:

1. **Download the Application**: Download the PaySky Assessment application to your local machine.

2. **Configure Database Connection**:
   - Open the application in Visual Studio or your preferred IDE.
   - Locate the `appsettings.json` file in the project.
   - Modify the connection string to point to your Microsoft SQL Server database. Replace `<DefaultConnection>` with your actual database connection details.

   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=<YourServer>;Database=<YourDatabase>;Trusted_Connection=True;"
   }
   ```

3. **Run the Application**:
   - Build and run the application in your IDE. This will automatically restore the necessary NuGet packages and start the development server.

4. **Access the Application**:
   - Once the application is running, open a web browser and navigate to the application's URL (typically `https://localhost:5001` or as configured).

5. **User Registration and Login**:
   - Use the self-registration feature to create user accounts for both Employer and Applicant user types.
   - Log in using the registered credentials.

6. **Application Usage**:
   - As an Employer:
     - Create, edit, and delete job vacancies.
     - Specify the maximum number of allowed applications for each vacancy.
     - Post and deactivate vacancies with expiry dates.
     - View the list of applicants for a specific vacancy.

   - As an Applicant:
     - Search for job vacancies.
     - Apply for job vacancies, ensuring not to exceed the maximum allowed applications.
     - Note that applicants are restricted from applying for more than one vacancy per day (within a 24-hour period).

7. **Archiving Mechanism**:
   - The application includes an archiving mechanism for expired vacancies to maintain data integrity.
8. **Architecture and Considerations**:
   - The application follows a traditional N-Tier Architecture.
   - MS SQL Server is used for the database.
   - Considerations have been made for caching, logging, and system security.


## Support and Feedback
If you encounter any issues, have questions, or would like to provide feedback, please contact the developer (Dillon Coleman) at [coleman399@gmail.com].

Thank you for exploring DillonColeman-PaySkyAssessment!
