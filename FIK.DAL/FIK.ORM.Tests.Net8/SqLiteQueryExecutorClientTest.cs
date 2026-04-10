using FIK.ORM.Tests.Share.Models;

namespace FIK.ORM.Tests.Net8;

public class SqLiteQueryExecutorClientTest
{
    [Fact]
    public void ShouldInsertSingleRecordInDefaultSchema_WhenNoSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        Items itemsTestData = new Items { Id = 1, Name = "Test", Price = 10.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "Id", itemsTestData.Id.ToString() } };

            //Act
            queryExecutorClient.Insert(itemsTestData, null, "Item");
            queryExecutorClient.CommitTransaction();
            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereColumns, null, null, "Item").SingleOrDefault();

            //Assert
            Assert.NotNull(actualItem);
            Assert.Equal(actualItem.Id, (itemsTestData.Id));
            Assert.Equal(actualItem.Name, (itemsTestData.Name));
            Assert.Equal(actualItem.Price, (itemsTestData.Price));
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertSingleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        Items itemsTestData = new Items { Id = 2, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 2, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item");
            queryExecutorClient.CommitTransaction();

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null).SingleOrDefault();

            //Assert
            Assert.NotNull(actualIvnItem);
            Assert.Equal(actualIvnItem.ItemId, (inventoryTestData.ItemId));
            Assert.Equal(actualIvnItem.Quantity, (inventoryTestData.Quantity));
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertMultipleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 3, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 4, Name = "Test 2", Price = 8.50m });

        List<Inventory> inventoryTestData = new List<Inventory>();
        inventoryTestData.Add(new Inventory { Id = 3, ItemId = itemsTestData[0].Id, Quantity = 110.5m });
        inventoryTestData.Add(new Inventory { Id = 4, ItemId = itemsTestData[1].Id, Quantity = 11 });

        try
        {
            //Arrange
            var whereClauseItem = $" Where Id IN (3,4) ";
            var whereClauseInvn = $" Where ItemId IN (3,4) ";

            //Act
            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item");
            queryExecutorClient.InsertBatch<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();

            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereClauseItem, null, null, "Item").Count();
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null).Count();

            //Assert
            Assert.Equal(actualIvnItem, (2));
            Assert.Equal(actualItem, (2));

        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.DeleteBatch<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldUpdateSingleRecordInDefaultSchema_WhenNoSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        Items itemsTestData = new Items { Id = 5, Name = "Test", Price = 10.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "Id", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item");
            queryExecutorClient.CommitTransaction();

            //Act
            itemsTestData.Price = 15.5m;
            queryExecutorClient.Update<Items>(itemsTestData, null, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();

            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereColumns, null, null, "Item").SingleOrDefault();

            //Assert
            Assert.NotNull(actualItem);
            Assert.Equal(actualItem.Id, (itemsTestData.Id));
            Assert.Equal(actualItem.Name, (itemsTestData.Name));
            Assert.Equal(actualItem.Price, (itemsTestData.Price));
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldUpdateSingleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        Items itemsTestData = new Items { Id = 6, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 6, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item");

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();


            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null).SingleOrDefault();
            inventoryTestData.Id = actualIvnItem.Id;
            inventoryTestData.Quantity = 120.5m;
            queryExecutorClient.Update<Inventory>(inventoryTestData, null, new string[] { "Id" }, null);
            queryExecutorClient.CommitTransaction();

            actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereColumns, null, null, null).SingleOrDefault();

            //Assert
            Assert.NotNull(actualIvnItem);
            Assert.Equal(actualIvnItem.ItemId, (inventoryTestData.ItemId));
            Assert.Equal(actualIvnItem.Quantity, (inventoryTestData.Quantity));
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();

        }
    }

    [Fact]
    public void ShouldUpdateMultipleRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 7, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 8, Name = "Test 2", Price = 8.50m });

        List<Inventory> inventoryTestData = new List<Inventory>();
        inventoryTestData.Add(new Inventory { Id = 7, ItemId = itemsTestData[0].Id, Quantity = 110.5m });
        inventoryTestData.Add(new Inventory { Id = 8, ItemId = itemsTestData[1].Id, Quantity = 11 });

        try
        {
            //Arrange
            var whereClauseItem = $" Where Id IN (7,8) ";
            var whereClauseInvn = $" Where ItemId IN (7,8) ";

            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item");
            queryExecutorClient.InsertBatch<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();

            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null);

            //Act
            foreach (var item in actualIvnItem)
            {
                var inventoryTestDataItem = inventoryTestData.Find(m => m.ItemId == item.ItemId);
                inventoryTestDataItem.Id = item.Id;
                inventoryTestDataItem.Quantity += 10;
            }
            queryExecutorClient.UpdateBatch<Inventory>(inventoryTestData, null, new string[] { "Id" }, null);
            queryExecutorClient.CommitTransaction();


            //Assert
            actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null);
            Assert.Equal(actualIvnItem.Count(), (2));

            foreach (var item in actualIvnItem)
            {
                var inventoryTestDataItem = inventoryTestData.Find(m => m.Id == item.Id);
                Assert.Equal(item.Quantity, (inventoryTestDataItem.Quantity));
                Assert.Equal(item.ItemId, (inventoryTestDataItem.ItemId));
            }

        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.DeleteBatch<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldInsertOrUpdateRecordInSchema_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        List<Items> itemsTestData = new List<Items>();
        itemsTestData.Add(new Items { Id = 9, Name = "Test", Price = 10.5m });
        itemsTestData.Add(new Items { Id = 10, Name = "Test 2", Price = 8.50m });

        List<Inventory> inventoryTestDatas = new List<Inventory>();
        inventoryTestDatas.Add(new Inventory { Id = 9, ItemId = itemsTestData[0].Id, Quantity = 110.5m });
        inventoryTestDatas.Add(new Inventory { Id = 10, ItemId = itemsTestData[1].Id, Quantity = 50.5m });


        try
        {
            //Arrange
            var whereClauseItem = $" Where Id IN (9,10) ";
            var whereClauseInvn = $" Where ItemId IN (9,10) ";

            queryExecutorClient.InsertBatch<Items>(itemsTestData, null, "Item");
            queryExecutorClient.InsertOrUpdate<Inventory>(inventoryTestDatas, null, new string[] { "Quantity" }, new string[] { "ItemId" }, null);
            queryExecutorClient.CommitTransaction();


            //Act
            // this will increase the Quantity of existing record by 5 as we have provided Quantity in updateColumn and Id in whereColumn and Id is already exist in database. If we provide any column other than Quantity in updateColumn then it will update the record with provided value instead of increasing it.
            inventoryTestDatas[0].Quantity = 5;
            queryExecutorClient.InsertOrUpdate<Inventory>(inventoryTestDatas[0], null, new string[] { "+Quantity" }, new string[] { "ItemId" }, null);
            queryExecutorClient.CommitTransaction();


            //Assert
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null).ToList();
            var actualItemCount = queryExecutorClient.Select<Items>(typeof(Items), null, whereClauseItem, null, null, "Item").Count();

            Assert.Equal(actualIvnItem[0].Quantity, (inventoryTestDatas[0].Quantity + 110.5m));
            Assert.Equal(actualIvnItem[0].ItemId, (inventoryTestDatas[0].ItemId));
            Assert.Equal(actualIvnItem[1].Quantity, (inventoryTestDatas[1].Quantity));
            Assert.Equal(actualItemCount, (2));

        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.DeleteBatch<Inventory>(inventoryTestDatas, new string[] { "ItemId" }, null);
            queryExecutorClient.DeleteBatch<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }

    [Fact]
    public void ShouldSelectDataTable_WhenNoTableNameProvidedAndSchemaProvided()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);
        Items itemsTestData = new Items { Id = 22, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 22, ItemId = itemsTestData.Id, Quantity = 110.5m };

        try
        {
            //Arrange
            var whereColumns = new Dictionary<string, string> { { "ItemId", itemsTestData.Id.ToString() } };
            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item");
            queryExecutorClient.CommitTransaction();

            //Act
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();

            var actualIvnDataTable = queryExecutorClient.Select(typeof(Inventory), null, whereColumns, null, null, null);
            var actualIvnDataTable2 = queryExecutorClient.Select(typeof(Inventory), null, " WHERE ItemId=22", null, null, null);

            //Assert
            Assert.NotNull(actualIvnDataTable);
            Assert.NotNull(actualIvnDataTable2);
            Assert.Equal(actualIvnDataTable.Rows.Count, (1));
            Assert.Equal(actualIvnDataTable2.Rows.Count, (1));
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            queryExecutorClient.Delete<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            queryExecutorClient.Delete<Items>(itemsTestData, new string[] { "Id" }, "Item");
            queryExecutorClient.CommitTransaction();
        }
    }



    [Fact]
    public void ShouldProcessCompositeModelInSchema()
    {

        var queryExecutorClient = new QueryExecutorClient(TestSettings.ConnectionStringSqlite, Enums.DatabaseProvider.Sqlite);

        var itemsTestData = new Items { Id = 11, Name = "Test", Price = 10.5m };
        Inventory inventoryTestData = new Inventory { Id = 11, ItemId = itemsTestData.Id, Quantity = 110.5m };
        Sales saleTestData = new Sales { Id = 100, ItemId = itemsTestData.Id, Quantity = 2, TotalPrice = 2 * itemsTestData.Price };


        try
        {
            //Arrange
            var whereClauseItem = $" Where Id = 11 ";
            var whereClauseInvn = $" Where ItemId =11 ";

            queryExecutorClient.Insert<Items>(itemsTestData, null, "Item");
            queryExecutorClient.Insert<Inventory>(inventoryTestData, null, null);
            queryExecutorClient.CommitTransaction();

            CompositeModelBuilder compositeModelBuilder = new CompositeModelBuilder();
            //define table name when you have different table name for same model in database
            compositeModelBuilder.AddInsertRecordSet<Sales>(saleTestData, null, "Sale");
            inventoryTestData.Quantity = saleTestData.Quantity;
            compositeModelBuilder.AddInsertOrUpdateRecordSet<Inventory>(inventoryTestData, null, new string[] { "-Quantity" }, new string[] { "ItemId" }, null);
            itemsTestData.Price = 15.5m;
            compositeModelBuilder.AddUpdateRecordSet<Items>(itemsTestData, new string[] { "Id" }, new string[] { "Price" }, "Item");

            compositeModelBuilder.AddRecordSetRawQuery("UPDATE Item Set price=16 WHERE Id=11 ");

            //Act
            queryExecutorClient.ExecuteCompositeModel(compositeModelBuilder);
            queryExecutorClient.CommitTransaction();

            //Assert
            var actualIvnItem = queryExecutorClient.Select<Inventory>(typeof(Inventory), null, whereClauseInvn, null, null, null).SingleOrDefault();
            var actualItem = queryExecutorClient.Select<Items>(typeof(Items), null, whereClauseItem, null, null, "Item").SingleOrDefault();
            var actualSales = queryExecutorClient.Select<Sales>(typeof(Sales), null, whereClauseInvn, null, null, "Sale").SingleOrDefault();

            Assert.Equal(actualIvnItem.Quantity, (110.5m - inventoryTestData.Quantity));
            Assert.Equal(actualIvnItem.ItemId, (inventoryTestData.ItemId));

            Assert.Equal(actualItem.Price, (itemsTestData.Price + .5m));

            Assert.Equal(actualSales.ItemId, (saleTestData.ItemId));
            Assert.Equal(actualSales.Quantity, (saleTestData.Quantity));
            Assert.Equal(actualSales.TotalPrice, (saleTestData.TotalPrice));

        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            CompositeModelBuilder compositeModelBuilder = new CompositeModelBuilder();
            compositeModelBuilder.AddDeleteRecordSet<Sales>(saleTestData, new string[] { "ItemId" }, "Sale");
            compositeModelBuilder.AddDeleteRecordSet<Inventory>(inventoryTestData, new string[] { "ItemId" }, null);
            compositeModelBuilder.AddDeleteRecordSet<Items>(itemsTestData, new string[] { "Id" }, "Item");

            queryExecutorClient.ExecuteCompositeModel(compositeModelBuilder);
            queryExecutorClient.CommitTransaction();

        }
    }
}
