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
    using System;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using VSSonarPlugins;

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

        /// <summary>Initializes a new instance of the <see cref="SolutionEventsListener"/> class.</summary>
        public SolutionEventsListener(IVsEnvironmentHelper helper)
        {
            this.environment = helper;
            this.InitNullEvents();

            this.solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            if (this.solution != null)
            {
                this.solution.AdviseSolutionEvents(this, out this.solutionEventsCookie);
            }
        }

        /// <summary>The on after open project.</summary>
        public event Action OnAfterOpenProject;

        /// <summary>The on query unload project.</summary>
        public event Action OnQueryUnloadProject;


        #region IDisposable Members

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            if (this.solution != null && this.solutionEventsCookie != 0)
            {
                GC.SuppressFinalize(this);
                this.solution.UnadviseSolutionEvents(this.solutionEventsCookie);
                this.OnQueryUnloadProject = null;
                this.OnAfterOpenProject = null;
                this.solutionEventsCookie = 0;
                this.solution = null;
            }
        }

        #endregion

        /// <summary>The init null events.</summary>
        private void InitNullEvents()
        {
            this.OnAfterOpenProject += () => { };
            this.OnQueryUnloadProject += () => { };
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
            string solutionName = this.environment.ActiveSolutionName();
            string solutionPath = this.environment.ActiveSolutionPath();

            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Solution Opened: " + solutionName + " : " + solutionPath);
            SonarQubeViewModelFactory.SQViewModel.AssociateProjectToSolution(solutionName, solutionPath);

            string text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return VSConstants.S_OK;
            }

            string fileName = this.environment.ActiveFileFullPath();
            SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(fileName);

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
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Solution Closed");
            SonarQubeViewModelFactory.SQViewModel.ClearProjectAssociation();
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
            return VSConstants.S_OK;
        }

        /// <summary>The on query unload project.</summary>
        /// <param name="pRealHierarchy">The p real hierarchy.</param>
        /// <param name="pfCancel">The pf cancel.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            this.OnQueryUnloadProject();
            return VSConstants.S_OK;
        }

        #endregion

    }
}