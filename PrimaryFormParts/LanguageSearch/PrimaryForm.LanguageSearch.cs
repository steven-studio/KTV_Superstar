using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace KTV_Superstar;

public partial class PrimaryForm
{
    // 通用的按钮初始化方法
    private void InitializeButton(ref Button button, string buttonText, int x, int y, int width, int height, Rectangle cropArea, Image normalBackground, out Bitmap normalBackgroundOut, Image activeBackground, out Bitmap activeBackgroundOut, EventHandler clickEventHandler)
    {
        button = new Button { Text = buttonText, Visible = false };
        ResizeAndPositionButton(button, x, y, width, height);
        normalBackgroundOut = new Bitmap(normalBackground).Clone(cropArea, normalBackground.PixelFormat);
        activeBackgroundOut = new Bitmap(activeBackground).Clone(cropArea, activeBackground.PixelFormat);
        button.BackgroundImage = normalBackgroundOut;
        button.BackgroundImageLayout = ImageLayout.Stretch;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Click += clickEventHandler;
        this.Controls.Add(button);
    }

    private Button languageSearchButton = default!;
    private Bitmap languageSearchNormalBackground = default!; // 語言搜索按钮的正常背景图像
    private Bitmap languageSearchActiveBackground = default!; // 語言搜索按钮的激活背景图像

    private Button guoYuButton = default!;
    private Bitmap guoYuNormalBackground = default!;
    private Bitmap guoYuActiveBackground = default!;
    private Button taiYuButton = default!;
    private Bitmap taiYuNormalBackground = default!;
    private Bitmap taiYuActiveBackground = default!;
    private Button yueYuButton = default!;
    private Bitmap yueYuNormalBackground = default!;
    private Bitmap yueYuActiveBackground = default!;
    private Button yingWenButton = default!;
    private Bitmap yingWenNormalBackground = default!;
    private Bitmap yingWenActiveBackground = default!;
    private Button riYuButton = default!;
    private Bitmap riYuNormalBackground = default!;
    private Bitmap riYuActiveBackground = default!;
    private Button hanYuButton = default!;
    private Bitmap hanYuNormalBackground = default!;
    private Bitmap hanYuActiveBackground = default!;
    private Button keYuButton = default!;
    private Bitmap keYuNormalBackground = default!;
    private Bitmap keYuActiveBackground = default!;

    private void LanguageSongSelectionButton_Click(object sender, EventArgs e)
    {
        newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
        hotPlayButton.BackgroundImage = hotPlayNormalBackground;
        singerSearchButton.BackgroundImage = singerSearchNormalBackground;
        songSearchButton.BackgroundImage = songSearchNormalBackground;
        languageSearchButton.BackgroundImage = languageSearchActiveBackground;
        groupSearchButton.BackgroundImage = groupSearchNormalBackground;
        categorySearchButton.BackgroundImage = categorySearchNormalBackground;
        orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
        myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
        promotionsButton.BackgroundImage = promotionsNormalBackground;
        deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;

        guoYuButton.BackgroundImage = guoYuActiveBackground;
        taiYuButton.BackgroundImage = taiYuNormalBackground;
        yueYuButton.BackgroundImage = yueYuNormalBackground;
        yingWenButton.BackgroundImage = yingWenNormalBackground;
        riYuButton.BackgroundImage = riYuNormalBackground;
        hanYuButton.BackgroundImage = hanYuNormalBackground;
        keYuButton.BackgroundImage = keYuNormalBackground;

        guoYuSongs = allSongs.Where(song => song.Category == "國語")
                            .OrderByDescending(song => song.Plays) // 根据点播次数降序排列
                            .ToList();
        currentPage = 0; // 重置到第一页
        currentSongList = guoYuSongs; // 更新当前显示的歌曲列表
        totalPages = (int)Math.Ceiling((double)guoYuSongs.Count / itemsPerPage);

        // DisplaySongsOnPage(currentSongList, currentPage);
        multiPagePanel.currentPageIndex = 0;
        multiPagePanel.LoadSongs(currentSongList);

        // 显示第三个图片，并可能隐藏第一个图片
        SetHotSongButtonsVisibility(false);
        SetNewSongButtonsVisibility(false);
        SetSingerSearchButtonsVisibility(false);
        SetSongSearchButtonsVisibility(false);
        SetGroupButtonsVisibility(false);
        SetPictureBoxCategoryAndButtonsVisibility(false);
        SetZhuYinSingersAndButtonsVisibility(false);
        SetZhuYinSongsAndButtonsVisibility(false);
        SetEnglishSingersAndButtonsVisibility(false);
        SetEnglishSongsAndButtonsVisibility(false);
        SetPinYinSingersAndButtonsVisibility(false);
        SetPinYinSongsAndButtonsVisibility(false);
        SetPictureBoxToggleLightAndButtonsVisibility(false);
        SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);
        SetPictureBoxLanguageButtonsVisibility(true);

        // 切换pictureBoxQRCode的可见性
        if (pictureBoxQRCode != null)
        {
            pictureBoxQRCode.Visible = false;
            closeQRCodeButton.Visible = false;
        }
    }

    private void OnLanguageButtonClick(Button activeButton, Image activeBackground, string category)
    {
        // 重置所有按钮的背景图像
        guoYuButton.BackgroundImage = guoYuNormalBackground;
        taiYuButton.BackgroundImage = taiYuNormalBackground;
        yueYuButton.BackgroundImage = yueYuNormalBackground;
        yingWenButton.BackgroundImage = yingWenNormalBackground;
        riYuButton.BackgroundImage = riYuNormalBackground;
        hanYuButton.BackgroundImage = hanYuNormalBackground;
        keYuButton.BackgroundImage = keYuNormalBackground;

        // 设置当前活动按钮的背景图像
        activeButton.BackgroundImage = activeBackground;

        // 获取指定类别的歌曲
        var selectedSongs = allSongs.Where(song => song.Category == category)
                                    .OrderByDescending(song => song.Plays) // 根据点播次数降序排列
                                    .ToList();
        currentPage = 0; // 重置到第一页
        currentSongList = selectedSongs; // 更新当前显示的歌曲列表
        totalPages = (int)Math.Ceiling((double)selectedSongs.Count / itemsPerPage);

        // DisplaySongsOnPage(currentSongList, currentPage);
        multiPagePanel.currentPageIndex = 0;
        multiPagePanel.LoadSongs(currentSongList);
    }

    private void SetPictureBoxLanguageButtonsVisibility(bool isVisible)
    {   
        guoYuButton.Visible = isVisible;
        guoYuButton.BringToFront();
        
        taiYuButton.Visible = isVisible;
        taiYuButton.BringToFront();
        
        yueYuButton.Visible = isVisible;
        yueYuButton.BringToFront();
        
        yingWenButton.Visible = isVisible;
        yingWenButton.BringToFront();
        
        riYuButton.Visible = isVisible;
        riYuButton.BringToFront();
        
        hanYuButton.Visible = isVisible;
        hanYuButton.BringToFront();

        keYuButton.Visible = isVisible;
        keYuButton.BringToFront();
    }

    private void InitializeButtonsForPictureBoxLanguageQuery()
    {
        InitializeGuoYuButton();
        InitializeTaiYuButton();
        InitializeYueYuButton();
        InitializeYingWenButton();
        InitializeRiYuButton();
        InitializeHanYuButton();
        InitializeKeYuButton();
    }
}