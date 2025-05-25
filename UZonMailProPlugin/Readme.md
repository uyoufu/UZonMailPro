# 开发文档

## 数据迁移

z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add addJsVariable --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add addJsVariable --context SqLiteContextPro --output-dir Migrations/SqLite -v

## 取消数据迁移

1. Mysql

dotnet ef migrations remove --context MysqlContextPro -v

2. SqLite

dotnet ef migrations remove --context SqLiteContextPro -v