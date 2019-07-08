//------------------------------------------------------------------------------
// <copyright file="GitForm.cs" company="LabSichen">
//     Copyright (c) LabSichen.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace DbGitAddIn
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("81a6fbaf-1c5d-4d12-a962-87764e265a2e")]
    public class GitForm : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitForm"/> class.
        /// </summary>
        public GitForm() : base(null)
        {
            this.Caption = "Git Tool";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GitFormControl();
        }
    }
}
