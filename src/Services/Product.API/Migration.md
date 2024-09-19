﻿- Migration commands:
	- Open: Click `View` => Select `Terminal`
	- Enter `cd src\Services`
	- Add Migration: `dotnet ef migrations add "InitDatabase" -p Product.API --startup-project Product.API -o Migrations`
	- Update Database: `dotnet ef database update -p Product.API --startup-project Product.API`
	- Remove Migration: `dotnet ef migrations remove --project Product.API --startup-project Product.API`