// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolutionEventsListener.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The solution events listener.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarQubeExtension
{
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using VSSonarPlugins;
    using VSSonarQubeExtension.Helpers;

    /// <summary>The solution events listener.</summary>
    public class SolutionEventsListener : IVsSolutionEvents, IDisposable
    {
        /// <summary>
        ///     The environment.
        /// </summary>
        private readonly IVsEnvironmentHelper environment;

        /// <summary>The solution.</summary>
        private IVsSolution solution;

        /// <summary>The solution events cookie.</summary>
        private uint solutionEventsCookie;
        private readonly IVsEnvironmentHelper visualStudioInterface;
        private readonly DTE2 dte2;
        private readonly VsSonarExtensionPackage vsSonarExtensionPackage;

        public VsEvents VsEvents { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="SolutionEventsListener"/> class.</summary>
        public SolutionEventsListener(IVsEnvironmentHelper helper)
        {

        }

        public SolutionEventsListener(IVsEnvironmentHelper helper, IVsEnvironmentHelper visualStudioInterface, DTE2 dte2, VsSonarExtensionPackage vsSonarExtensionPackage) : this(helper)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.visualStudioInterface = visualStudioInterface;
            this.dte2 = dte2;
            this.vsSonarExtensionPackage = vsSonarExtensionPackage;

            environment = helper;
            InitNullEvents();

            solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            if (VsEvents == null)
            {
                VsEvents = new VsEvents(this.visualStudioInterface, this.dte2, this.vsSonarExtensionPackage);
            }
        }

#pragma warning disable CS0414 // not assigned
        /// <summary>The on after open project.</summary>
        public event Action OnAfterOpenProject;
#pragma warning restore CS0414 // not assigned

        /// <summary>The on query unload project.</summary>
        public event Action OnQueryUnloadProject;


        #region IDisposable Members

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (solution != null && solutionEventsCookie != 0)
            {
                GC.SuppressFinalize(this);
                solution.UnadviseSolutionEvents(solutionEventsCookie);
                OnQueryUnloadProject = null;
                OnAfterOpenProject = null;
                solutionEventsCookie = 0;
                solution = null;
            }
        }

        #endregion

        /// <summary>The init null events.</summary>
        private void InitNullEvents()
        {
            OnAfterOpenProject += () => { };
            OnQueryUnloadProject += () => { };
        }

        #region IVsSolutionEvents Members

        /// <summary>The on after close solution.</summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        /// <summary>The on after load project.</summary>
        /// <param name="pStubHierarchy">The p stub hierarchy.</param>
        /// <param name="pRealHierarchy">The p real hierarchy.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>The on after open project.</summary>
        /// <param name="pHierarchy">The p hierarchy.</param>
        /// <param name="fAdded">The f added.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            //this.OnAfterOpenProject();
            return VSConstants.S_OK;
        }

        /// <summary>The on after open solution.</summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <param name="fNewSolution">The f new solution.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            string solutionName = environment.ActiveSolutionFileNameWithExtension();
            string solutionPath = environment.ActiveSolutioRootPath();
            string fileName = environment.ActiveFileFullPath();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Opened: " + solutionName + " : " + solutionPath);
#pragma warning disable VSTHRD110 // Observe result of async calls
            System.Threading.Tasks.Task.Run(async () => await SonarQubeViewModelFactory.SQViewModel.OnSolutionOpen(solutionName, solutionPath, fileName));
#pragma warning restore VSTHRD110 // Observe result of async calls
            return VSConstants.S_OK;
        }

        /// <summary>The on before close project.</summary>
        /// <param name="pHierarchy">The p hierarchy.</param>
        /// <param name="fRemoved">The f removed.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        /// <summary>The on before close solution.</summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Closed");
#pragma warning disable VSTHRD110 // Observe result of async calls
            System.Threading.Tasks.Task.Run(async () => await SonarQubeViewModelFactory.SQViewModel.OnSolutionClosed());
#pragma warning restore VSTHRD110 // Observe result of async calls
            return VSConstants.S_OK;
        }

        /// <summary>The on before unload project.</summary>
        /// <param name="pRealHierarchy">The p real hierarchy.</param>
        /// <param name="pStubHierarchy">The p stub hierarchy.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>The on query close project.</summary>
        /// <param name="pHierarchy">The p hierarchy.</param>
        /// <param name="fRemoving">The f removing.</param>
        /// <param name="pfCancel">The pf cancel.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        /// <summary>The on query close solution.</summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <param name="pfCancel">The pf cancel.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            pfCancel = SonarQubeViewModelFactory.SQViewModel.CanCloseSolution();
            return VSConstants.S_OK;
        }

        /// <summary>The on query unload project.</summary>
        /// <param name="pRealHierarchy">The p real hierarchy.</param>
        /// <param name="pfCancel">The pf cancel.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            OnQueryUnloadProject();
            return VSConstants.S_OK;
        }

        #endregion
    }
}