using FIK.ORM.Tests.Share.Models;
using System.Security.Policy;
using static System.Net.Mime.MediaTypeNames;


namespace FIK.ORM.Tests.Net8;


public class PostgreSQLQueryExecutorClientTest
{

    [Fact]
    public void ShouldInsertSingleBlobData()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);

        byte[] imageBytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\20210727_124357.png");

        ItemBlob itemsTestData = new ItemBlob { Id = 1, BlobData= imageBytes };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "Id", itemsTestData.Id.ToString() } };

            //Act
            queryExecutorClient.Insert(itemsTestData, null, "ItemBlob", "public");
            queryExecutorClient.CommitTransaction();
            var actualItem = queryExecutorClient.Select<ItemBlob>(typeof(ItemBlob), null, whereColumns, null, null, "ItemBlob", "public").SingleOrDefault();

            //Assert
            Assert.NotNull(actualItem);
            Assert.Equal(itemsTestData.Id, actualItem.Id);
            Assert.Equal(itemsTestData.BlobData, actualItem.BlobData);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<ItemBlob>(itemsTestData, new string[] { "Id" }, "ItemBlob", "public");
            queryExecutorClient.CommitTransaction();
        }
    }


    [Fact]
    public void ShouldInsertSingleRecordInDefaultSchema_WhenNoSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        Items itemsTestData = new Items { Id = 1, Name = "Test", Price = 10.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "Id", itemsTestData.Id.ToString() } };

            //Act
            queryExecutorClient.Insert(itemsTestData, null, "Item", "public");
            queryExecutorClient.CommitTransaction();
            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereColumns, null, null, "Item", "public").SingleOrDefault();

            //Assert
            Assert.NotNull(actualItem);
            Assert.Equal(itemsTestData.Id, actualItem.Id);
            Assert.Equal(itemsTestData.Name, actualItem.Name);
            Assert.Equal(itemsTestData.Price, actualItem.Price);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertSingleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        Items itemsTestData = new Items { Id = 2, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 2, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.CommitTransaction();

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null, "dboInvn").SingleOrDefault();

            //Assert
            Assert.NotNull(actualIvnItem);
            Assert.Equal(inventoryTestData.ItemId, actualIvnItem.ItemId);
            Assert.Equal(inventoryTestData.Quantity, actualIvnItem.Quantity);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertMultipleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 3, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 4, Name = "Test 2", Price = 8.50m });

        List<Inventory> inventoryTestData = new List<Inventory>();
        inventoryTestData.Add(new Inventory { ItemId = itemsTestData[0].Id, Quantity = 110.5m });
        inventoryTestData.Add(new Inventory { ItemId = itemsTestData[1].Id, Quantity = 11 });

        try
        {
            //Arrange
            var whereClauseItem = $" Where \"Id\" IN (3,4) ";
            var whereClauseInvn = $" Where \"ItemId\" IN (3,4) ";

            //Act
            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.InsertBatch<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            var actualItem = queryExecutorClient.Select<Items>(typeof(Inventory), null, whereClauseItem, null, null, "Item", "public").Count();
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null, "dboInvn").Count();

            //Assert
            Assert.Equal(2, actualIvnItem);
            Assert.Equal(2, actualItem);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.DeleteBatch<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldUpdateSingleRecordInDefaultSchema_WhenNoSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        Items itemsTestData = new Items { Id = 5, Name = "Test", Price = 10.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "Id", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.CommitTransaction();

            //Act
            itemsTestData.Price = 15.5m;
            queryExecutorClient.Update<Items>(itemsTestData, null, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();

            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereColumns, null, null, "Item", "public").SingleOrDefault();

            //Assert
            Assert.NotNull(actualItem);
            Assert.Equal(itemsTestData.Id, actualItem.Id);
            Assert.Equal(itemsTestData.Name, actualItem.Name);
            Assert.Equal(itemsTestData.Price, actualItem.Price);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldUpdateSingleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        Items itemsTestData = new Items { Id = 6, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 6, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item", "public");

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null, "dboInvn").SingleOrDefault();
            inventoryTestData.Id = actualIvnItem.Id;
            inventoryTestData.Quantity = 120.5m;
            queryExecutorClient.Update<Inventory>(inventoryTestData, null, new string[] { "Id" }, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null, "dboInvn").SingleOrDefault();

            //Assert
            Assert.NotNull(actualIvnItem);
            Assert.Equal(inventoryTestData.ItemId, actualIvnItem.ItemId);
            Assert.Equal(inventoryTestData.Quantity, actualIvnItem.Quantity);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldUpdateMultipleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 7, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 8, Name = "Test 2", Price = 8.50m });

        List<Inventory> inventoryTestData = new List<Inventory>();
        inventoryTestData.Add(new Inventory { ItemId = itemsTestData[0].Id, Quantity = 110.5m });
        inventoryTestData.Add(new Inventory { ItemId = itemsTestData[1].Id, Quantity = 11 });

        try
        {
            //Arrange
            var whereClauseItem = $" Where \"Id\" IN (7,8) ";
            var whereClauseInvn = $" Where \"ItemId\" IN (7,8) ";

            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.InsertBatch<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null, "dboInvn");

            //Act
            foreach (var item in actualIvnItem)
            {
                var inventoryTestDataItem = inventoryTestData.Find(m => m.ItemId == item.ItemId);
                inventoryTestDataItem.Id = item.Id;
                inventoryTestDataItem.Quantity += 10;
            }
            queryExecutorClient.UpdateBatch<Inventory>(inventoryTestData, null, new string[] { "Id" }, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            //Assert
            actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null, "dboInvn");
            Assert.Equal(2, actualIvnItem.Count());

            foreach (var item in actualIvnItem)
            {
                var inventoryTestDataItem = inventoryTestData.Find(m => m.Id == item.Id);
                Assert.Equal(inventoryTestDataItem.Quantity, item.Quantity);
                Assert.Equal(inventoryTestDataItem.ItemId, item.ItemId);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.DeleteBatch<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertOrUpdateRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 9, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 10, Name = "Test 2", Price = 8.50m });

        Inventory inventoryTestData = new Inventory { ItemId = itemsTestData[0].Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereClauseItem = $" Where \"Id\" IN (9,10) ";
            var whereClauseInvn = $" Where \"ItemId\" =9 ";

            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.InsertOrUpdate<Inventory>(inventoryTestData, null, new string[] { "Quantity" }, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            //Act
            inventoryTestData.Quantity = 5;
            queryExecutorClient.InsertOrUpdate<Inventory>(inventoryTestData, null, new string[] { "+Quantity" }, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            //Assert
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null, "dboInvn").SingleOrDefault();
            var actualItemCount = queryExecutorClient.Select<Items>(typeof(Items), null, whereClauseItem, null, null, "Item", "public").Count();

            Assert.Equal(inventoryTestData.Quantity + 110.5m, actualIvnItem.Quantity);
            Assert.Equal(inventoryTestData.ItemId, actualIvnItem.ItemId);
            Assert.Equal(2, actualItemCount);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldSelectDataTable_WhenNoTableNameProvidedAndSchemaProvided()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);
        Items itemsTestData = new Items { Id = 22, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 22, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.CommitTransaction();

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            var actualIvnDataTable = queryExecutorClient.Select(typeof(Inventory), null, whereColumns, null, null, null, "dboInvn");
            var actualIvnDataTable2 = queryExecutorClient.Select(typeof(Inventory), null, " WHERE \"ItemId\"=22", null, null, null, "dboInvn");

            //Assert
            Assert.NotNull(actualIvnDataTable);
            Assert.NotNull(actualIvnDataTable2);
            Assert.Equal(1, actualIvnDataTable.Rows.Count);
            Assert.Equal(1, actualIvnDataTable2.Rows.Count);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldProcessCompositeModelInSchema()
    {
        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringPostgreSQL, Enums.DatabaseProvider.PostgreSQL);

        var itemsTestData = new Items { Id = 11, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { ItemId = itemsTestData.Id, Quantity = 110.5m };
        Sales saleTestData = new Sales { Id = 100, ItemId = itemsTestData.Id, Quantity = 2, TotalPrice = 2 * itemsTestData.Price };

        try
        {
            //Arrange
            var whereClauseItem = $" Where \"Id\" = 11 ";
            var whereClauseInvn = $" Where \"ItemId\" = 11 ";

            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item", "public");
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null, "dboInvn");
            queryExecutorClient.CommitTransaction();

            CompositeModelBuilder compositeModelBuilder = new CompositeModelBuilder();
            compositeModelBuilder.AddInsertRecordSet<Sales>(saleTestData, null, "Sale", "public");
            inventoryTestData.Quantity = saleTestData.Quantity;
            compositeModelBuilder.AddInsertOrUpdateRecordSet<Inventory>(inventoryTestData, null, new string[] { "-Quantity" }, new string[] { "ItemId" }, null, "dboInvn");
            itemsTestData.Price = 15.5m;
            compositeModelBuilder.AddUpdateRecordSet<Items>(itemsTestData, new string[] { "Id" }, new string[] { "Price" }, "Item", "public");

            compositeModelBuilder.AddRecordSetRawQuery("UPDATE \"public\".\"Item\" Set \"Price\"=16 WHERE \"Id\"=11 ");

            //Act
            queryExecutorClient.ExecuteCompositeModel(compositeModelBuilder);
            queryExecutorClient.CommitTransaction();

            //Assert
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null, "dboInvn").SingleOrDefault();
            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereClauseItem, null, null, "Item", "public").SingleOrDefault();
            var actualSales = queryExecutorClient.Select<Sales>(typeof(Sales), null, whereClauseInvn, null, null, "Sale", "public").SingleOrDefault();

            Assert.NotNull(actualItem);
            Assert.NotNull(actualIvnItem);
            Assert.NotNull(actualSales);
            Assert.Equal(16m, actualItem.Price);
            Assert.Equal(saleTestData.Quantity, actualSales.Quantity);
            Assert.Equal(itemsTestData.Id, actualSales.ItemId);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Sales>(saleTestData, new string[] { "ItemId" }, "Sale", "public");
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null, "dboInvn");
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item", "public");
            queryExecutorClient.CommitTransaction();
        }
    }
}
