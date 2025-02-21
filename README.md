# Software Engineering II - Talent Management Platform for IT Professionals #

## Requirements ## 

 * You need to install the Microsoft .NET SDK 8 available at https://dotnet.microsoft.com/en-us/download
 * This solution was tested with version 8.0.6

## Basic Setup ## 

 * Fill .env file (you can copy or adapt from .env.sample if you want to run things locally) 
 * Run the database with Docker
 * Update the database by running the current migrations 

## Migrations ##

 * To create a new migration you can do `dotnet ef migrations add MigrationName`
 * To update the database you can do `dotnet ef database update`

## Default Data ##

 * Roles: Admin, Normal
 * User: root@example.com, Password: root (Admin)
 * User: normal@example.com, Password: normal (Normal)
 * Permissions:
   * Sample Feature 1 (Normal and Admin)
   * Sample Feature 2 (Normal and Admin)
   * Sample Admin Feature (Admin)

## Notes / Rules and Recommendations ##

 * ⚠️ **Never push the .env file to git** 
 * `docker compose up --build database` will allow you to create a local database for you to work

## TODO ##

 * The docker compose containers for the frontend and api are not working yet!

## Git commands:

1. Clone the repository:
```bash
git clone git@github.com:gabrielaRibeiro1/es2_d2_c.git
```
2. Create a new branch for your feature:
```bash
git checkout -b feature/login
```
3. Add your files and push them into github:
```bash
git add .
git commit -m "Fix issue in login feature"
git push origin feature/login
```
## Create a Pull Request (PR)

1. Go to the GitHub repository.

2. Click **"Compare & pull request"** for your branch (`feature/login`).

3. Add a description of your changes.

4. **Set the base branch** to `main` and the compare branch to `feature/login`.

5. Click **"Create pull request"**.

6. **Request a review**: Assign one person to review your PR.

## Useful links ##
* [Trello Dashboard - Sprint](https://trello.com/b/5ubS10uf/es2-d2-tema-c)

