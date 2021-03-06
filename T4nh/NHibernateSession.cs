using System;
using NHibernate;

namespace Genius.Northwind.BusinessServices
{
    public class NHibernateSession
    {
        #region Declarations

        protected ITransaction transaction = null;
        protected ISession iSession;

        #endregion

        #region Constructor & Destructor

        public NHibernateSession(ISession session)
        {
            this.iSession = session;
        }
        ~NHibernateSession()
        {
            Dispose(true);
        }

        #endregion

        #region IDisposable

        private bool _isDisposed = false;
        public void Dispose()
        {
            Dispose(false);
        }
        private void Dispose(bool finalizing)
        {
            if (!_isDisposed)
            {
                // Close Session
                Close();

                // Flag as disposed.
                _isDisposed = true;
                if (!finalizing)
                    GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Methods

        public void CommitChanges()
        {
            if (HasOpenTransaction)
                CommitTransaction();
            else
                iSession.Flush();
        }
        public void Close()
        {
            if (iSession.IsOpen)
            {
                iSession.Close();
            }
        }

        public bool BeginTransaction()
        {
            bool result = !HasOpenTransaction;
            if (result)
                transaction = iSession.BeginTransaction();
            return result;
        }
        public bool CommitTransaction()
        {
            bool result = HasOpenTransaction;
            if (result)
            {
                try
                {
                    transaction.Commit();
                    transaction = null;
                }
                catch (HibernateException)
                {
                    transaction.Rollback();
                    transaction = null;
                    throw;
                }
            }
            return result;
        }
        public bool RollbackTransaction()
        {
            bool result = HasOpenTransaction;
            if (result)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;

                // I dont know why, but it seems that after you rollback a transaction you need to reset the session.
                // Personally, I dislike this; I find it inefficent, and it means that I have to expose a method to
                // get an ISession from the NHibernateSessionManager...does anyone know how to get around this problem?
                iSession.Close();
                iSession.Dispose();
                //iSession = NHibernateSessionManager.Instance.CreateISession();
            }
            return result;
        }

        public ISession GetISession()
        {
            return iSession;
        }

        #endregion

        #region Properties

        public bool HasOpenTransaction
        {
            get
            {
                return (transaction != null && !transaction.WasCommitted && !transaction.WasRolledBack);
            }
        }
        public bool IsOpen
        {
            get { return iSession.IsOpen; }
        }

        #endregion
    }
}
