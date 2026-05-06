IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'InventraFtsCatalog')
    CREATE FULLTEXT CATALOG InventraFtsCatalog AS DEFAULT;

IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Inventories'))
    CREATE FULLTEXT INDEX ON Inventories(Title, Description) 
    KEY INDEX PK_Inventories 
    ON InventraFtsCatalog 
    WITH (CHANGE_TRACKING = AUTO);

IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Items'))
    CREATE FULLTEXT INDEX ON Items(
        CustomId, CustomString1Value, CustomString2Value, 
        CustomString3Value, CustomText1Value, CustomText2Value, CustomText3Value
    ) 
    KEY INDEX PK_Items 
    ON InventraFtsCatalog 
    WITH (CHANGE_TRACKING = AUTO);