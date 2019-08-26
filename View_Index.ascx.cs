using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Entities.Modules;

using DotNetNuke.Common;



namespace DNNGo.Modules.DNNGalleryPro
{
    public partial class View_Index : BaseModule, DotNetNuke.Entities.Modules.IActionable
    {
        #region "属性"



 
        /// <summary>
        /// 当前标签
        /// </summary>
        public String Token
        {
            get { return WebHelper.GetStringParam(HttpContext.Current.Request, String.Format("Token{0}",ModuleId), "List").ToLower(); }
        } 




        #endregion


        #region "事件"

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                //加载脚本
                LoadViewScript();


                if (!IsPostBack)
                {

                    panNavigation.Visible = IsEdit;

                 
                    hlManager.NavigateUrl = xUrl();
                    //hlManager.Visible = UserId > 0 &&  ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", ModuleConfiguration);
                    hlNewItem.NavigateUrl = xUrl("AddNew");
                    //hlNewArticle.Visible = UserId > 0 && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", ModuleConfiguration);


               



                }


            } 
            catch (Exception exc) //Module failed to load
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        protected void Page_Init(System.Object sender, System.EventArgs e)
        {
            try
            {

    

                //绑定Tabs和容器中的控件
                BindContainer();

            }
            catch (Exception exc) //Module failed to load
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        #endregion

        #region "方法"

 

        /// <summary>
        /// 绑定列表数据到容器
        /// </summary>
        private void BindContainer()
        {
            //加载相应的控件
            PortalModuleBase ManageContent = new PortalModuleBase();
            String ModuleSrc = "View_List.ascx";
            if (Token == "info")
            {
                ModuleSrc = "View_Info.ascx";
            }
 
            ManageContent.ID = Token;
            String ContentSrc = ResolveClientUrl(string.Format("{0}/{1}", this.TemplateSourceDirectory, ModuleSrc));
            ManageContent = (PortalModuleBase)LoadControl(ContentSrc);
            ManageContent.ModuleConfiguration = this.ModuleConfiguration;
            ManageContent.LocalResourceFile = Localization.GetResourceFile(this, string.Format("{0}.resx", ModuleSrc));
            phContainer.Controls.Add(ManageContent);
        }

        #endregion



        #region Optional Interfaces
        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection Actions = new ModuleActionCollection();

                Actions.Add(this.GetNextActionID(), Localization.GetString("NewItem", this.LocalResourceFile), ModuleActionType.AddContent, "", "settings.gif", xUrl("AddNew"), false, SecurityAccessLevel.Edit, true, false);
                Actions.Add(this.GetNextActionID(), Localization.GetString("Manager", this.LocalResourceFile), ModuleActionType.AddContent, "", "settings.gif", xUrl("ManagerList"), false, SecurityAccessLevel.Edit, true, false);
                Actions.Add(this.GetNextActionID(), Localization.GetString("Options", this.LocalResourceFile), ModuleActionType.AddContent, "", "settings.gif", xUrl("Effect_Options"), false, SecurityAccessLevel.Edit, true, false);


                return Actions;
            }
        }
        #endregion
    }
}