#!/bin/bash

rm -rf Migrations
dotnet ef migrations add init
dotnet ef database drop
dotnet ef database update