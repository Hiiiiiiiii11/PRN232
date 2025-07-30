@echo off
echo Updating remaining hardcoded API URLs...

echo.
echo This script will help update the remaining hardcoded localhost:7045 URLs.
echo.
echo The following files still need manual updates:
echo - DrugUserPreventionUI/Pages/Appointment.cshtml.cs
echo - DrugUserPreventionUI/Pages/ProgranDetail.cshtml.cs  
echo - DrugUserPreventionUI/Pages/StaffDashboard/StaffDashboard.cshtml.cs
echo - DrugUserPreventionUI/Pages/ManageProgram/ManageProgram.cshtml.cs
echo - DrugUserPreventionUI/Pages/ManagerDashboard/ManagerDashboard.cshtml.cs
echo - DrugUserPreventionUI/Pages/NewsArticles/NewsArticles.cshtml.cs
echo - DrugUserPreventionUI/Pages/NewsArticles/MyNewsArticle.cshtml.cs
echo - DrugUserPreventionUI/Pages/Courses/MyCourses.cshtml.cs
echo - DrugUserPreventionUI/Pages/Courses/Courses.cshtml.cs
echo - DrugUserPreventionUI/Pages/CourseDashboard/CourseDashboard.cshtml.cs
echo - DrugUserPreventionUI/Pages/AdminDashboard/AdminDashboard.cshtml.cs
echo.
echo Key changes needed:
echo 1. Add "using DrugUserPreventionUI.Configuration;" to imports
echo 2. Add "private readonly ApiConfiguration _apiConfig;" field
echo 3. Add ApiConfiguration parameter to constructor
echo 4. Replace hardcoded URLs with _apiConfig properties
echo.
echo Press any key to continue...
pause