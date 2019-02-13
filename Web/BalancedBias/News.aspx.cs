﻿using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BalancedBias.Common.Config;
using BalancedBias.Common.Connectivity;
using BalancedBias.Common.Constants;
using BalancedBias.Common.Infrastructure;
using BalancedBias.Common.Rss;

public partial class News : System.Web.UI.Page
{
    private static readonly IChannelsDbService ChannelsDbService = DependencyResolverGateway.Resolve<IChannelsDbService>();
    private static readonly IRssChannelsService RssChannelsService = DependencyResolverGateway.Resolve<IRssChannelsService>();

    /// <summary>
    /// MediaBasePath property of type string
    /// </summary>
    public string MediaBasePath;

    /// <summary>
    /// Public constructor for News aspx class
    /// </summary>
    public News()
        : this(DependencyResolverGateway.Resolve<IAppConfigReader>())
    { }

    /// <inheritdoc />
    /// <summary>
    /// Private constructor for News aspx class
    /// </summary>
    /// <param name="appConfigReader"></param>
    private News(IAppConfigReader appConfigReader)
    {
        MediaBasePath = appConfigReader.AppConfigToString(AppSettingKeys.MediaBasePath);
    }

    /// <summary>
    /// Loads News aspx class page dependencies
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;
        var todaysDate = DateTime.Today.ToString(CultureInfo.InvariantCulture).Split(null)[0];
        datesDropDown.Text = todaysDate;
        var allChannels = ChannelsDbService.GetChannelsFromDbByDate(todaysDate);
        BindAllChannels(allChannels);

        var allDates = ChannelsDbService.GetUniqueArticleDates().OrderByDescending(date => date);
        datesDropDown.DataSource = allDates;
        datesDropDown.DataBind();
    }

    /// <summary>
    /// Gets articles from DB by date from datesDropDown selection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void SearchArticlesByDate(object sender, EventArgs e)
    {
        var date = datesDropDown.Text;
        var allChannels = ChannelsDbService.GetChannelsFromDbByDate(date);
        BindAllChannels(allChannels);
    }
    
    /// <summary>
    /// Binds data for all channels with articles loaded
    /// </summary>
    /// <param name="allChannels"></param>
    private void BindAllChannels(NewsCollection allChannels)
    {
        RssChannelsService.PersistNewArticles();
        gvRss.DataSource = allChannels.Channels;
        gvRss.DataBind();
    }

    /// <summary>
    /// Binds articles to their template dynamically, based on template types
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnArticleDataBound(object sender, RepeaterItemEventArgs e)
    {
        var article = (Article)e.Item.DataItem;
        var channel = article.Channel;
        var templateName = RssChannelsServiceSection.GetTemplateByChannelName(channel);
        var articleTemplateControl = (dynamic)Page.LoadControl("~/ArticleTemplates/" + templateName + ".ascx");
        var placeholder = e.Item.FindControl("ArticlePlaceHolder") as PlaceHolder;
        articleTemplateControl.ArticleTemplate = (Article)e.Item.DataItem;
        articleTemplateControl.PathToStyles = MediaBasePath + "/ArticleTemplates/" + templateName + ".css";
        if (placeholder != null) placeholder.Controls.Add(articleTemplateControl);
    }
}