using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UIForia.DataSource;

#pragma warning disable 1998,0649

[TestFixture]
public class DataSourceTests {

    private class TestException : Exception { }

    private class TestData : IRecord {

        public string data;

        public TestData(long id, string data) {
            Id = id;
            this.data = data;
        }

        public long Id { get; set; }

    }

    private class TestAdapter<T> : Adapter<T> where T : class, IRecord {

        public Action<T> addRecordHandler;
        public Action<long, T> getRecordHandler;
        public Func<T, T, bool> recordChanged;
       
        public override async Task<T> AddRecord(T record) {
            addRecordHandler?.Invoke(record);
            return record;
        }

        public override async Task<T> GetRecord(long id, T currentRecord) {
            getRecordHandler?.Invoke(id, currentRecord);
            return currentRecord;
        }

        public override bool RecordChanged(T a, T b) {
            if (recordChanged == null) return base.RecordChanged(a, b);
            return recordChanged.Invoke(a, b);
        }

    }
    
// having issues with deadlock, figure this out later
//    [Test]
//    public void AddRecord_WaitForAdapterConfig() {
//        TestAdapterWithConfig<TestData> adapter = new TestAdapterWithConfig<TestData>();
//        DataSource<TestData> ds = new DataSource<TestData>(adapter);
//        TestData data = ds.AddRecord(new TestData(0, "wait")).Result;
//        Assert.AreEqual(1, adapter.configureCount);
//    }

    [Test]
    public void AddRecord_UseInternalStore() {
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();
        int getCall = 0;
        adapter.getRecordHandler = (long id, TestData record) => {
            if (getCall == 0) {
                Assert.IsNull(record);
            }
            else if (getCall == 1) {
                Assert.AreEqual("hello", record.data);
            }

            getCall++;
        };

        DataSource<TestData> ds = new DataSource<TestData>(adapter);
        TestData d0 = ds.GetRecord(0).Result;
        Assert.IsNull(d0);
        Assert.AreEqual(1, getCall);
        TestData d1 = ds.AddRecord(new TestData(0, "hello")).Result;
        TestData d2 = ds.GetRecord(0).Result;
        Assert.AreEqual(2, getCall);
    }

    [Test]
    public void AddRecord_AndGet() {
        DataSource<TestData> ds = new DataSource<TestData>();
        TestData d0 = ds.AddRecord(new TestData(0, "hello")).Result;
        TestData d1 = ds.AddRecord(new TestData(1, "world")).Result;

        TestData r0 = ds.GetRecord(0).Result;
        TestData r1 = ds.GetRecord(1).Result;

        Assert.AreEqual(r0, d0);
        Assert.AreEqual(r1, d1);
    }

    [Test]
    public void AddRecord_AddNullReturnsNull() {
        DataSource<TestData> ds = new DataSource<TestData>();
        Assert.IsNull(ds.AddRecord(null).Result);
    }
    
    [Test]
    public void AddRecord_EmitEventOnRecordAdded() {
        DataSource<TestData> ds = new DataSource<TestData>();
        TestData add0 = null;
        TestData add1 = null;
        int addCalls = 0;
        ds.onRecordAdded += (TestData t) => {
            if (addCalls == 0) {
                add0 = t;
            }
            else if (addCalls == 1) {
                add1 = t;
            }

            addCalls++;
        };

        TestData d0 = ds.AddRecord(new TestData(0, "hello")).Result;
        TestData d1 = ds.AddRecord(new TestData(1, "world")).Result;
        Assert.AreEqual(add0, d0);
        Assert.AreEqual(add1, d1);
        Assert.AreEqual(2, addCalls);
    }

    [Test]
    public void AddRecord_AddsLotsOfRecords() {
        DataSource<TestData> ds = new DataSource<TestData>();
        int addCalls = 0;
        ds.onRecordAdded += (TestData t) => {
            addCalls++;
        };
        for (int i = 0; i < 100; i++) {
            TestData result = ds.AddRecord(new TestData(i, i.ToString())).Result;
        }
        
        Assert.AreEqual(100, ds.RecordCount);
        Assert.AreEqual(100, addCalls);
    }

    [Test]
    public void AddRecord_CanThrowFromAdapter() {
        var ex = Assert.Throws<AggregateException >(() => {
            DataSource<TestData> ds = new DataSource<TestData>();
            ds.onRecordAdded += (TestData t) => {
               throw new TestException();
            };
            TestData result = ds.AddRecord(new TestData(0, "throw")).Result;
        });
        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.IsInstanceOf<TestException>(ex.InnerExceptions[0]);
    }
//
//    [Test]
//    public void UpsertRecord() {
//        
//        TestAdapter<TestData> adapter = new TestAdapter<TestData>();
//        
//        adapter.recordChanged = (a, b) => a.data != b.data;
//        
//        DataSource<TestData> ds = new DataSource<TestData>(adapter);
//        
//        int addCalls = 0;
//        int changeCalls = 0;
//        
//        ds.onRecordAdded += (record) => { addCalls++; };
//        ds.onRecordChanged += (record) => { changeCalls++; };
//        
//        TestData result = ds.UpsertRecord(new TestData(0, "value0")).Result;
//        Assert.AreEqual(1, addCalls);
//        result = ds.UpsertRecord(new TestData(0, "value1")).Result;
//        Assert.AreEqual(1, addCalls);
//        result = ds.GetRecord(0).Result;
//        Assert.AreEqual("value1", result.data);
//        Assert.AreEqual(1, changeCalls);
//    }

    [Test]
    public void SetRecord_RemoveIfValueIsNull() {
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();

        adapter.recordChanged = (a, b) => a.data != b.data;

        int addCount = 0;
        int changeCount = 0;
        int removeCount = 0;
        
        DataSource<TestData> ds = new DataSource<TestData>(adapter);
        
        ds.onRecordAdded += (r) => { addCount++; };
        ds.onRecordChanged += (r) => { changeCount++; };
        ds.onRecordRemoved += (r) => { removeCount++; };
        
        var result1 = ds.SetRecord(0, new TestData(0, "hello")).Result;
        Assert.AreEqual(0, removeCount);
        Assert.AreEqual(0, changeCount);
        Assert.AreEqual(1, addCount);
        var result2 = ds.SetRecord(0, null).Result;
        Assert.AreEqual(1, addCount);
        Assert.AreEqual(1, removeCount);
        var result3 = ds.GetRecord(0).Result;
        Assert.IsNull(result3);
    }

    [Test]
    public void SetRecord_AddRecordToStore() {
        
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();
        ListRecordStore<TestData> store = new ListRecordStore<TestData>();
        DataSource<TestData> ds = new DataSource<TestData>(adapter, store);
        var result = ds.SetRecord(100, new TestData(100, "data here")).Result;
        Assert.AreEqual(result, store.GetRecord(100));
    }

    [Test]
    public void SetRecord_EmitRecordAddedWhenAdded() {
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();

        adapter.recordChanged = (a, b) => a.data != b.data;

        int addCount = 0;
        DataSource<TestData> ds = new DataSource<TestData>(adapter);
        ds.onRecordAdded += (r) => { addCount++; };
        
        var result1 = ds.SetRecord(0, new TestData(0, "hello")).Result;
        Assert.AreEqual(1, addCount);
        var result2 = ds.SetRecord(0, new TestData(0, "goodbye")).Result;
        Assert.AreEqual(1, addCount);
    }

    [Test]
    public void SetRecord_EmitRecordChangedWhenChanged() {
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();

        adapter.recordChanged = (a, b) => a.data != b.data;
        int changeCount = 0;
        DataSource<TestData> ds = new DataSource<TestData>(adapter);
        ds.onRecordChanged += (r) => { changeCount++; };
        var result1 = ds.SetRecord(0, new TestData(0, "hello")).Result;
        Assert.AreEqual(0, changeCount);
        var result2 = ds.SetRecord(0, new TestData(0, "goodbye")).Result;
    }

    [Test]
    public void SetRecord_EmitRecordRemovedWhenRemoved() {
        TestAdapter<TestData> adapter = new TestAdapter<TestData>();

        adapter.recordChanged = (a, b) => a.data != b.data;
        int removedCount = 0;
        DataSource<TestData> ds = new DataSource<TestData>(adapter);
        ds.onRecordRemoved += (r) => { removedCount++; };
        var result1 = ds.SetRecord(0, new TestData(0, "hello")).Result;
        Assert.AreEqual(0, removedCount);
        var result2 = ds.SetRecord(0, null);
        Assert.AreEqual(1, removedCount);
    }
    

}