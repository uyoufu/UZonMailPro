# 开发文档

## 数据迁移

z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add fixManyToMany --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add fixManyToMany --context SqLiteContextPro --output-dir Migrations/SqLite -v

## 取消数据迁移

1. Mysql

dotnet ef migrations remove --context MysqlContext -v

2. SqLite

dotnet ef migrations remove --context SqLiteContext -v