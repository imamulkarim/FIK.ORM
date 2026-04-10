
CREATE TABLE [Item] (
  [Id] bigint NOT NULL
, [Name] text NOT NULL
, [Price] numeric(18,4) NULL
, CONSTRAINT [sqlite_master_PK_item] PRIMARY KEY ([Id])
);


/****** Object:  Table [dbo].[Sale]    Script Date: 4/7/2026 4:49:47 PM ******/

CREATE TABLE [Sale] (
  [Id] bigint NOT NULL
, [ItemId] bigint NOT NULL
, [Quantity] numeric(18,4) NOT NULL
, [TotalPrice] numeric(18,4) NULL
, CONSTRAINT [sqlite_master_PK_sale] PRIMARY KEY ([Id])
);


CREATE TABLE [Inventory] (
  [Id] bigint NOT NULL
, [ItemId] bigint NOT NULL
, [Quantity] numeric(18,4) NOT NULL
, CONSTRAINT [sqlite_master_PK_inventory] PRIMARY KEY ([Id])
);

