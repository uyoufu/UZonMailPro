# �����ĵ�

## ����Ǩ��

z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add fixManyToMany --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add fixManyToMany --context SqLiteContextPro --output-dir Migrations/SqLite -v

## ȡ������Ǩ��

1. Mysql

dotnet ef migrations remove --context MysqlContext -v

2. SqLite

dotnet ef migrations remove --context SqLiteContext -v