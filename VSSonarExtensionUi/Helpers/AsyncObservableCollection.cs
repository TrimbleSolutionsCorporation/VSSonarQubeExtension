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
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows.Threading;

    /// <summary>
    ///     The async observable collection.
    /// </summary>
    /// <typeparam>
    ///     <name>T</name>
    /// </typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        #region Public Events

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Methods

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            if (CollectionChanged != null)
            {
                foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                {
                    var dispObj = nh.Target as DispatcherObject;
                    if (dispObj != null)
                    {
                        Dispatcher dispatcher = dispObj.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            dispatcher.BeginInvoke(
                                (Action)(() => nh.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }
                    nh.Invoke(this, e);
                }
            }
        }

        #endregion
    }
}