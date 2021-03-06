﻿using System;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Web;
using DevExpress.XtraReports.Web.QueryBuilder;
using DevExpress.XtraReports.Web.ReportDesigner;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using SecurityBestPractices.Authorization;
using SecurityBestPractices.Authorization.Dashboards;
using SecurityBestPractices.Authorization.Reports;

namespace SecurityBestPractices
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e) {
            #region Query builder
            DefaultQueryBuilderContainer.Register<IDataSourceWizardConnectionStringsProvider, DataSourceWizardConnectionStringsProvider>();
            DefaultQueryBuilderContainer.RegisterDataSourceWizardDBSchemaProviderExFactory<DataSourceWizardDBSchemaProviderExFactory>();

            //DevExpress.XtraReports.Web.ASPxQueryBuilder.StaticInitialize(); // This line is unnecessary if ASPxReportDesigner.StaticInitialize() has been called
            #endregion

            #region Reports
            DefaultWebDocumentViewerContainer.Register<IExportingAuthorizationService, OperationLogger>();
            DefaultWebDocumentViewerContainer.Register<WebDocumentViewerOperationLogger, OperationLogger>();
            DefaultWebDocumentViewerContainer.Register<IWebDocumentViewerAuthorizationService, OperationLogger>();

            DefaultReportDesignerContainer.Register<WebDocumentViewerOperationLogger, OperationLogger>();

            DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportStorageWithAccessRules());
            DefaultReportDesignerContainer.RegisterDataSourceWizardConnectionStringsProvider<DataSourceWizardConnectionStringsProvider>(); // Provide connections to Report Designer
            DefaultReportDesignerContainer.RegisterDataSourceWizardDBSchemaProviderExFactory<DataSourceWizardDBSchemaProviderExFactory>(); // Provide only nessesary dbtables

            DevExpress.XtraReports.Web.WebDocumentViewer.Native.WebDocumentViewerBootstrapper.SessionState = System.Web.SessionState.SessionStateBehavior.Required;
            DevExpress.XtraReports.Web.ASPxReportDesigner.StaticInitialize();
            #endregion           

            #region Dashboards
            var service = new DashboardStorageWithAccessRules();
            DashboardConfigurator.Default.SetDashboardStorage(service);
            DashboardConfigurator.Default.CustomParameters += (o, args) => {
                if (!service.IsAuthorized(args.DashboardId))
                    throw new UnauthorizedAccessException();
            };

            DashboardConfigurator.Default.SetConnectionStringsProvider(new DataSourceWizardConnectionStringsProvider()); // Provide connections to Dashboard Designer
            DashboardConfigurator.Default.SetDBSchemaProvider(new DBSchemaProviderEx()); // Provide only nessesary dbtables
            #endregion            
        }
    }
}