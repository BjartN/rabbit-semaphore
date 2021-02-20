using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading;
using System.Transactions;

namespace SqlServerLock.StartUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var lockmode = ReadInput();

            var locker  = new SqlApplicationLock("locked_resource", ConfigurationManager.AppSettings["ConnectionString"]);
            Console.WriteLine("Aquiring the lock");

            try
            {
                using (locker.TakeLock(TimeSpan.FromMilliseconds(10), lockmode))
                {
                    Console.WriteLine($"{lockmode.ToString()} Lock Aquired. Press any key to release the lock.");
                    Console.ReadKey();
                }

                Console.WriteLine("Lock Released");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fail");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static SqlApplicationLock.LockMode ReadInput()
        {
            Console.WriteLine("1) Exclusive lock");
            Console.WriteLine("2) Shared lock");
            var number = Console.ReadLine();
            var lockmode = number == "1" ? SqlApplicationLock.LockMode.Exclusive : SqlApplicationLock.LockMode.Shared;
            return lockmode;
        }
    }

    public class SqlApplicationLock : IDisposable
    {
       
        private readonly string _uniqueId;
        private readonly SqlConnection _sqlConnection;
        private bool _isLockTaken = false;

        public SqlApplicationLock(
            string uniqueId,
            string connectionString)
        {
            _uniqueId = uniqueId;
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public IDisposable TakeLock(TimeSpan takeLockTimeout, LockMode lockMode)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand sqlCommand = new SqlCommand("sp_getapplock", _sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = (int)takeLockTimeout.TotalSeconds;

                sqlCommand.Parameters.AddWithValue("Resource", _uniqueId);
                sqlCommand.Parameters.AddWithValue("LockOwner", "Session");
                sqlCommand.Parameters.AddWithValue("LockMode", lockMode.ToString());
                sqlCommand.Parameters.AddWithValue("LockTimeout", (Int32)takeLockTimeout.TotalMilliseconds);

                SqlParameter returnValue = sqlCommand.Parameters.Add("ReturnValue", SqlDbType.Int);
                returnValue.Direction = ParameterDirection.ReturnValue;
                sqlCommand.ExecuteNonQuery();

                if ((int)returnValue.Value < 0)
                {
                    throw new Exception(String.Format("sp_getapplock failed with errorCode '{0}'", returnValue.Value));
                }

                _isLockTaken = true;

                transactionScope.Complete();
            }

            return this;
        }

        public void ReleaseLock()
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand sqlCommand = new SqlCommand("sp_releaseapplock", _sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("Resource", _uniqueId);
                sqlCommand.Parameters.AddWithValue("LockOwner", "Session");

                sqlCommand.ExecuteNonQuery();
                _isLockTaken = false;
                transactionScope.Complete();
            }
        }

        public void Dispose()
        {
            if (_isLockTaken)
            {
                ReleaseLock();
            }
            _sqlConnection.Close();
        }

        public enum LockMode
        {
            Exclusive,
            Shared
        }
    }
}
