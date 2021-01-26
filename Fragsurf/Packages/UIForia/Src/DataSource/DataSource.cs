using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIForia.Util;

namespace UIForia.DataSource {

    public sealed class DataSource<T> where T : class, IRecord {

        private Task config;
        private readonly Adapter<T> adapter;
        private readonly IRecordStore<T> recordStore;

        public event Action<T> onRecordAdded;
        public event Action<T> onRecordChanged;
        public event Action<T> onRecordRemoved;

        public DataSource(Adapter<T> adapter = null, IRecordStore<T> store = null) {
            this.adapter = adapter ?? new Adapter<T>();
            this.recordStore = store ?? new ListRecordStore<T>();
            this.config = this.adapter.Configure(recordStore);
        }

        public int RecordCount => recordStore.Count;

        public async Task<ICollection<T>> LoadRecords(ICollection<T> output = null) {
            if (config != null) {
                await config;
                config = null;
            }

            return AddRecordsToStore(await adapter.LoadRecords(output));
        }

        public ICollection<T> SyncLoadRecords(ICollection<T> output = null) {
            WaitForConfig();
           
            return AddRecordsToStore(adapter.SyncLoadRecords(output));
        }

        private ICollection<T> AddRecordsToStore(ICollection<T> output) {
            if (output == null) return output;
            
            foreach (T returned in output) {
                if (returned == null) {
                    continue;
                }

                T local = recordStore.GetRecord(returned.Id);

                if (local == null) {
                    onRecordAdded?.Invoke(returned);
                }
                else {
                    onRecordChanged?.Invoke(returned);
                }

                recordStore.SetRecord(returned);
            }

            return output;
        }

        private void WaitForConfig() {
            if (config != null) {
                // this blocks but is only executed once
                config.Wait();
                config = null;
            }
        }

        public async Task<T> AddRecord(T record) {
            if (record == null) {
                return null;
            }

            if (config != null) {
                await config;
                config = null;
            }

            T result = await adapter.AddRecord(record);

            if (result == null) {
                return null;
            }

            recordStore.SetRecord(record);
            this.onRecordAdded?.Invoke(record);
            return record;
        }

        public T SyncAddRecord(T record) {
            if (record == null) {
                return null;
            }

            WaitForConfig();

            T result = adapter.SyncAddRecord(record);

            if (result == null) {
                return null;
            }

            recordStore.SetRecord(record);
            this.onRecordAdded?.Invoke(record);
            return record;
        }

        public async Task<T> SetRecord(long id, T record) {
            if (record == null) {
                return await RemoveRecord(id);
            }

            if (config != null) {
                await config;
                config = null;
            }

            T localRecord = recordStore.GetRecord(id);
            T newRecord = await adapter.SetRecord(id, record, localRecord);

            UpdateRecordStore(record, newRecord, localRecord);

            return newRecord;
        }

        public T SyncSetRecord(long id, T record) {
            if (record == null) {
                return SyncRemoveRecord(id);
            }

            WaitForConfig();
            
            T localRecord = recordStore.GetRecord(id);
            T newRecord = adapter.SyncSetRecord(id, record, localRecord);

            UpdateRecordStore(record, newRecord, localRecord);

            return newRecord;
        }

        private void UpdateRecordStore(T record, T newRecord, T localRecord) {
            if (newRecord == null) {
                T current = recordStore.RemoveRecord(record.Id);
                if (current != null) {
                    onRecordRemoved?.Invoke(current);
                }
            }
            else if (localRecord == null) {
                recordStore.SetRecord(record);
                this.onRecordAdded?.Invoke(record);
            }
            else {
                recordStore.SetRecord(record);
                onRecordChanged?.Invoke(record);
            }
        }

        public async Task<T> RemoveRecord(long id) {
            if (config != null) {
                await config;
                config = null;
            }

            T localRecord = recordStore.GetRecord(id);
            T returnedRecord = await adapter.RemoveRecord(id, localRecord);
            localRecord = recordStore.RemoveRecord(id);
            if (localRecord != null) {
                onRecordRemoved?.Invoke(returnedRecord);
            }

            return returnedRecord;
        }

        public T SyncRemoveRecord(long id) {
            WaitForConfig();
            
            T localRecord = recordStore.GetRecord(id);
            T returnedRecord = adapter.SyncRemoveRecord(id, localRecord);
            localRecord = recordStore.RemoveRecord(id);
            if (localRecord != null) {
                onRecordRemoved?.Invoke(returnedRecord);
            }

            return returnedRecord;
        }

        public async Task<T> RemoveRecord(T record) {
            return await RemoveRecord(record.Id);
        }

        public async Task<T> GetRecord(long id) {
            T returnedRecord = await adapter.GetRecord(id, recordStore.GetRecord(id));

            if (GetRecordInternal(id, returnedRecord)) return null;
            return returnedRecord;
        }

        public T SyncGetRecord(long id) {
            T returnedRecord = adapter.SyncGetRecord(id, recordStore.GetRecord(id));

            if (GetRecordInternal(id, returnedRecord)) return null;
            return returnedRecord;
        }

        private bool GetRecordInternal(long id, T returnedRecord) {
            if (returnedRecord == null) {
                T removed = recordStore.RemoveRecord(id);
                if (removed != null) {
                    onRecordRemoved?.Invoke(removed);
                }

                return true;
            }

            T localRecord = recordStore.GetRecord(id);

            if (localRecord == null) {
                onRecordAdded?.Invoke(returnedRecord);
            }
            else if (adapter.RecordChanged(localRecord, returnedRecord)) {
                onRecordChanged?.Invoke(returnedRecord);
            }

            recordStore.SetRecord(returnedRecord);
            return false;
        }

        public void ClearStore() {

            if (onRecordRemoved == null) {
                recordStore.Clear();
                return;
            }
            
            LightList<T> records = LightList<T>.Get();
            records.EnsureCapacity(recordStore.Count);
            recordStore.GetAllRecords(records);
            T[] recordsArray = records.Array;
            recordStore.Clear();
            for (int i = 0; i < records.Count; i++) {
                onRecordRemoved.Invoke(recordsArray[i]);
            }

            LightList<T>.Release(ref records);
        }
    }

}