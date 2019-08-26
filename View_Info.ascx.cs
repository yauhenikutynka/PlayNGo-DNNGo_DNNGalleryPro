using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules.Actions;
using System.Collections;
using System.Collections.Specialized;

namespace DNNGo.Modules.DNNGalleryPro
{
    public partial class View_Info : BaseModule 
    {


        #region "扩展属性"


        public Int32 SliderID
        {
            get { return WebHelper.GetIntParam(Request, String.Format("SliderID{0}", Settings_ModuleID), 0); }
        }
         
        

        #endregion


        #region "方法"

 


        /// <summary>
        /// 绑定数据项到前台
        /// </summary>
        public void BindDataItem(EffectDBEntity EffectDB)
        {

            Hashtable Puts = new Hashtable();
            TemplateFormat xf = new TemplateFormat(this);
            xf.PhContent = PhContent;


            DNNGo_DNNGalleryPro_Slider SliderItem = DNNGo_DNNGalleryPro_Slider.FindByKeyForEdit(SliderID);


            Puts.Add("ThemeName", Settings_EffectThemeName);
            Puts.Add("SliderItem", SliderItem);
            Puts.Add("EffectName", Settings_EffectName);

            Puts.Add("LayerList", EffectDB.Layers ? GetLayerList() : new List<DNNGo_DNNGalleryPro_Layer>());
            liContent.Text = HttpUtility.HtmlDecode(ViewTemplate(EffectDB, "Layer.html", Puts, xf));


        }

        /// <summary>
        /// 分组列表
        /// </summary>
        /// <returns></returns>
        public List<DNNGo_DNNGalleryPro_Group> GetGroupList()
        {
            QueryParam qp = new QueryParam();
            qp.Orderfld = DNNGo_DNNGalleryPro_Group._.Sort;
            qp.OrderType = 0;
            int RecordCount = 0;
            qp.Where.Add(new SearchParam(DNNGo_DNNGalleryPro_Group._.ModuleId, Settings_ModuleID, SearchType.Equal));
            return DNNGo_DNNGalleryPro_Group.FindAll(qp, out RecordCount);
        }

        /// <summary>
        /// Layer列表
        /// </summary>
        /// <returns></returns>
        public List<DNNGo_DNNGalleryPro_Layer> GetLayerList()
        {
            QueryParam qp = new QueryParam();
            qp.Orderfld = DNNGo_DNNGalleryPro_Layer._.Sort;
            qp.OrderType = 0;
            int RecordCount = 0;
            qp.Where.Add(new SearchParam(DNNGo_DNNGalleryPro_Layer._.ModuleId, Settings_ModuleID, SearchType.Equal));
            qp.Where.Add(new SearchParam(DNNGo_DNNGalleryPro_Layer._.Status, (Int32)EnumStatus.Activated, SearchType.Equal));
            qp.Where.Add(new SearchParam(DNNGo_DNNGalleryPro_Layer._.SliderID, SliderID, SearchType.Equal));
            
            return DNNGo_DNNGalleryPro_Layer.FindAll(qp, out RecordCount);
        }


        #endregion



        #region "事件"
 
        /// <summary>
        /// 页面加载
        /// </summary>
        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
               
                

                if (!String.IsNullOrEmpty(Settings_EffectName))
                {

                    if (!String.IsNullOrEmpty(Settings_EffectThemeName))
                    {
                         EffectDBEntity EffectDB = Setting_EffectDB;
                        if (!IsPostBack)
                        {
                            //绑定数据项到前台
                            BindDataItem(EffectDB);

                        }

                        //是否支持谷歌地图
                        if (EffectDB.GoogleMap)
                        {
                            BindJavaScriptFile("GoogleMap", "https://maps.google.com/maps/api/js?sensor=false");
                        }
 
                        //需要载入当前设置效果的主题CSS文件
                        String ThemeName = String.Format("{0}_{1}", Settings_EffectName, Settings_EffectThemeName);
                        String ThemePath = String.Format("{0}Effects/{1}/Themes/{2}/Style.css", ModulePath, Settings_EffectName, Settings_EffectThemeName);
                        BindStyleFile(ThemeName, ThemePath);

                        //绑定效果中配置的样式和脚本
                        BindXmlDBToPage(EffectDB, "Effect");
                       

                    }
                    else
                    {
                        //未定义效果对应的主题
                        liContent.Text = "";
                    }
                }
                else
                {
                    //未绑定效果
                    liContent.Text = "";
                }
 
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }

        }

 

        #endregion

 
    }
}