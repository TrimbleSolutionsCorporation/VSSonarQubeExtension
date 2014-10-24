// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncObservableCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The async observable collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;

    /// <summary>
    /// The async observable collection.
    /// </summary>
    /// <typeparam>
    ///     <name>T</name>
    /// </typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        #region Fields

        /// <summary>
        /// The _synchronization context.
        /// </summary>
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        public AsyncObservableCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The clear items.
        /// </summary>
        protected override void ClearItems()
        {
            this.ExecuteOnSyncContext(() => base.ClearItems());
        }

        /// <summary>
        /// The insert item.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        protected override void InsertItem(int index, T item)
        {
            this.ExecuteOnSyncContext(() => base.InsertItem(index, item));
        }

        /// <summary>
        /// The move item.
        /// </summary>
        /// <param name="oldIndex">
        /// The old index.
        /// </param>
        /// <param name="newIndex">
        /// The new index.
        /// </param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            this.ExecuteOnSyncContext(() => base.MoveItem(oldIndex, newIndex));
        }

        /// <summary>
        /// The remove item.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        protected override void RemoveItem(int index)
        {
            this.ExecuteOnSyncContext(() => base.RemoveItem(index));
        }

        /// <summary>
        /// The set item.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        protected override void SetItem(int index, T item)
        {
            this.ExecuteOnSyncContext(() => base.SetItem(index, item));
        }

        /// <summary>
        /// The execute on sync context.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        private void ExecuteOnSyncContext(Action action)
        {
            if (SynchronizationContext.Current == this.synchronizationContext)
            {
                action();
            }
            else
            {
                this.synchronizationContext.Send(_ => action(), null);
            }
        }

        #endregion
    }
}