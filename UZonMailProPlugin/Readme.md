## 数据迁移说明

z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add initPro --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add initPro --context SqLiteContextPro --output-dir Migrations/SqLite -v