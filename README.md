1. ensure that csv file is in executing directory
2. you can use Postman collection to test the API
3. to switch CSV to SQLite define `"ActiveDataSource": "EF"` in appsettings.json

P.S: At first, I had an implementation that did not modify the CSV file, but when I added POST, I forgot about the requirement and started modifying the CSV file...

p.p.s: this is not a complex API, if you want to find more complex API, check out my Various APIs repository on github. 
You will find there DTO's, filters, paging, authorization, Unit of Work, etc.
