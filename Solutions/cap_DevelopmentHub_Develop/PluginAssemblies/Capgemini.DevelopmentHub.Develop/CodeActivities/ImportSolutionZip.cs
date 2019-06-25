﻿namespace Capgemini.DevelopmentHub.Develop.CodeActivities
{
    using System;
    using System.Activities;
    using System.Net;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Extensions;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Import a solution into an environment.
    /// </summary>
    [CrmPluginRegistration(
        nameof(ImportSolutionZip),
        FriendlyName,
        Description,
        Group,
        IsolationModeEnum.Sandbox)]
    public class ImportSolutionZip : IntegratedWorkflowActivity
    {
        /// <summary>
        /// The group of the workflow activity.
        /// </summary>
        public const string Group = "Capgemini.DevelopmentHub.Develop";

        /// <summary>
        /// The friendly name of the workflow activity.
        /// </summary>
        public const string FriendlyName = "Import solution zip";

        /// <summary>
        /// The description of the workflow activity.
        /// </summary>
        public const string Description = "Imports a solution zip file as a Base64 encoded string.";

        /// <summary>
        /// Gets or sets the solution zip as a Base64 encoded string.
        /// </summary>
        [RequiredArgument]
        [Input("Solution zip")]
        public InArgument<string> SolutionZip { get; set; }

        /// <summary>
        /// Gets or sets true if the import was successful or false if the import failed.
        /// </summary>
        [Output("Succeeded")]
        public OutArgument<bool> IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message encountered when importing (if any).
        /// </summary>
        [Output("Error")]
        public OutArgument<string> Error { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IODataClient oDataClient, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var solutionZip = this.SolutionZip.GetRequired(context, nameof(this.SolutionZip));
            var oDataSolutionService = context.GetExtension<IODataSolutionService>() ?? new ODataSolutionService(new ODataRepositoryFactory(oDataClient), logWriter);

            ImportJobData importJobData = null;
            try
            {
                importJobData = oDataSolutionService.ImportSolutionZip(Convert.FromBase64String(solutionZip)).Result;
            }
            catch (AggregateException ex) when (ex.InnerException is WebException)
            {
                this.Error.Set(context, ex.InnerException.Message);
                this.IsSuccessful.Set(context, false);
                return;
            }

            this.IsSuccessful.Set(context, importJobData.ImportResult == ImportResult.Success);
            this.Error.Set(context, importJobData.ErrorText);
        }
    }
}