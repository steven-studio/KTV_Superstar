using DirectShowLib;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KTV_Superstar;

public class MediaRenderer : IDisposable
{
    #region Properties for Primary Graph

    // Graph and control objects for Primary (主螢幕)
    public IGraphBuilder GraphBuilderPrimary { get; set; } = default!;
    public IMediaControl MediaControlPrimary { get; set; } = default!;
    public IVideoWindow VideoWindowPrimary { get; set; } = default!;
    private IMediaEventEx MediaEventExPrimary { get; set; } = default!;

    #endregion

    #region Properties for Secondary Graph
    public IGraphBuilder GraphBuilderSecondary { get; set; } = default!;
    public IMediaControl MediaControlSecondary { get; set; } = default!;
    public IVideoWindow VideoWindowSecondary { get; set; } = default!;
    private IMediaEventEx MediaEventExSecondary { get; set; } = default!;
    #endregion

    #region Common Filters (可根據需求決定是否設為 static)
    public IBaseFilter VideoRendererPrimary { get; private set; } = default!;
    public IBaseFilter VideoRendererSecondary { get; private set; } = default!;
    public IBaseFilter LavSplitterPrimary { get; private set; } = default!;
    public IBaseFilter LavSplitterSecondary { get; private set; } = default!;
    public IBaseFilter LavVideoDecoderPrimary { get; private set; } = default!;
    public IBaseFilter LavVideoDecoderSecondary { get; private set; } = default!;
    public IBaseFilter LavAudioDecoderSecondary { get; private set; } = default!;
    public IPin OutputPinSecondary { get; private set; } = default!;
    public IBaseFilter AudioRenderer { get; private set; } = default!;
    #endregion
    
    public bool IsPaused { get; set; } = false;
    public static bool IsInitializationComplete { get; private set; } = false;

    // 播放狀態（也可以使用狀態機模式進行封裝）
    public enum PlaybackState { Idle, Loading, Playing, Paused, Stopped }
    public PlaybackState CurrentState { get; private set; } = PlaybackState.Idle;

    #region Constructor & Disposal
    public MediaRenderer()
    {
        InitializeGraphBuilders();
    }

    public void Dispose()
    {
        StopAndReleaseResources();
        // 也可以釋放 GraphBuilder 等 COM 物件（依需求而定）
    }
    #endregion

    #region Graph Builders Initialization

    public void InitializeGraphBuilders() {
        // 封裝 InitializeGraphBuilderPrimary 與 InitializeGraphBuilderSecondary 的邏輯
        InitializeGraphBuilderPrimary();
        InitializeGraphBuilderSecondary();
    }

    private void InitializeGraphBuilderPrimary() {
        // 實現原本 VideoPlayerForm 中有關 primary graph 的邏輯
        GraphBuilderPrimary = (IGraphBuilder) new FilterGraph();
        // … 加入過濾器、配置 IMediaControl、IVideoWindow 等
        if (GraphBuilderPrimary == null) {
            Console.WriteLine("Failed to create FilterGraph for primary monitor.");
            throw new Exception("Failed to create FilterGraph for primary monitor.");
        }
        try {
            LavSplitterPrimary = AddFilterByClsid(GraphBuilderPrimary, "LAV Splitter", Clsid.LAVSplitter);

            LavVideoDecoderPrimary = AddFilterByClsid(GraphBuilderPrimary, "LAV Video Decoder", Clsid.LAVVideoDecoder);

            VideoRendererPrimary = AddFilterByClsid(GraphBuilderPrimary, "Primary Video Renderer", Clsid.VideoRenderer);
            int hr = GraphBuilderPrimary.AddFilter(VideoRendererPrimary, "Primary Video Renderer");
            DsError.ThrowExceptionForHR(hr);

            MediaControlPrimary = (IMediaControl) GraphBuilderPrimary;
            if (MediaControlPrimary == null) {
                Console.WriteLine("Failed to get Media Control for primary monitor.");
                return;
            }

            MediaEventExPrimary = (IMediaEventEx) GraphBuilderPrimary;
            if (MediaEventExPrimary == null) {
                Console.WriteLine("Failed to get Media Event Ex for primary monitor.");
                return;
            }
        } catch (Exception ex) {
            Console.WriteLine("Error initializing graph builder for primary monitor: " + ex.Message);
        }
    }

    private void InitializeGraphBuilderSecondary() {
        // 實現 secondary graph 的初始化
        GraphBuilderSecondary = (IGraphBuilder) new FilterGraph();
        // … 加入過濾器、配置 IMediaControl、IVideoWindow 等
        if (GraphBuilderSecondary == null) {
            Console.WriteLine("Failed to create FilterGraph");
            throw new Exception("Failed to create FilterGraph");
        }
        try {
            LavSplitterSecondary = AddFilterByClsid(GraphBuilderSecondary, "LAV Splitter", Clsid.LAVSplitter);
            LavVideoDecoderSecondary = AddFilterByClsid(GraphBuilderSecondary, "LAV Video Decoder", Clsid.LAVVideoDecoder);
            LavAudioDecoderSecondary = AddFilterByClsid(GraphBuilderSecondary, "LAV Audio Decoder", Clsid.LAVAudioDecoder);
            OutputPinSecondary = FindPin(LavAudioDecoderSecondary, "Output");
            VideoRendererSecondary = AddFilterByClsid(GraphBuilderSecondary, "Secondary Video Renderer", Clsid.VideoRenderer);
            if (VideoRendererSecondary == null) {
                Console.WriteLine("Failed to initialize Secondary Video Renderer.");
                return;
            }
            int hr = GraphBuilderSecondary.AddFilter(VideoRendererSecondary, "Secondary Video Renderer");
            DsError.ThrowExceptionForHR(hr);
            var clsidAudioRenderer = new Guid("79376820-07D0-11CF-A24D-0020AFD79767"); // CLSID for DirectSound Renderer
            AudioRenderer = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(clsidAudioRenderer)!)!;
            hr = GraphBuilderSecondary.AddFilter(AudioRenderer, "Default DirectSound Device");
            DsError.ThrowExceptionForHR(hr);

            MediaControlSecondary = (IMediaControl) GraphBuilderSecondary;
            if (MediaControlSecondary == null) {
                Console.WriteLine("Failed to get Media Control");
                return;
            }
            MediaEventExSecondary = (IMediaEventEx) GraphBuilderSecondary;
            if (MediaEventExSecondary == null) {
                Console.WriteLine("Failed to get Media Event Ex");
                return;
            }

            IsInitializationComplete = true;
        } catch (Exception ex) {
            Console.WriteLine("Error initializing graph builder with second monitor: " + ex.Message);
        }
    }

    #endregion

    #region Filter and Pin Helpers

    private static IBaseFilter AddFilterByClsid(IGraphBuilder graphBuilder, string name, Guid clsid) {
        try {
            // 获取 CLSID 对应的类型
            Type filterType = Type.GetTypeFromCLSID(clsid)!;
            Console.WriteLine($"Attempting to create filter of type: {filterType.FullName}");

            // 创建实例
            object filterObject = Activator.CreateInstance(filterType)!;

            // 尝试转换为 IBaseFilter
            IBaseFilter filter = (IBaseFilter)filterObject;

            if (filter == null) {
                // 如果转换失败，使用 IUnknown 获取并转换为 IBaseFilter
                IntPtr comObjectPointer = Marshal.GetIUnknownForObject(filterObject);
                filter = (IBaseFilter) Marshal.GetObjectForIUnknown(comObjectPointer);
                // Console.WriteLine($"Successfully converted COM object to IBaseFilter via IUnknown.");
            } else {
                // Console.WriteLine($"Successfully created IBaseFilter directly.");
            }

            // 添加过滤器到图形构建器
            int hr = graphBuilder.AddFilter(filter, name);
            if (hr != 0) {
                // Console.WriteLine($"Failed to add filter {name} with CLSID {clsid}, HRESULT: {hr}");
            }

            DsError.ThrowExceptionForHR(hr);
            // Console.WriteLine($"Successfully added filter {name} with CLSID {clsid}");
            return filter;
        } catch (Exception ex) {
            Console.WriteLine($"Exception in AddFilterByClsid: {ex.Message}");
            throw; // Rethrow the exception to handle it further up the call stack
        }
    }

    private IPin FindPin(IBaseFilter filter, string pinName) {
        IEnumPins enumPins;
        IPin[] pins = new IPin[1];

        filter.EnumPins(out enumPins);
        enumPins.Reset();

        while (enumPins.Next(1, pins, IntPtr.Zero) == 0) {
            PinInfo pinInfo;
            pins[0].QueryPinInfo(out pinInfo);
            Console.WriteLine(pinInfo);

            if (pinInfo.name == pinName) {
                return pins[0];
            }
        }
        return null!;
    }

    public static void RemoveAllFilters(IGraphBuilder graph) {
        IEnumFilters enumFilters;
        graph.EnumFilters(out enumFilters);
        IBaseFilter[] filters = new IBaseFilter[1];
        while (enumFilters.Next(1, filters, IntPtr.Zero) == 0) {
            graph.RemoveFilter(filters[0]);
            Marshal.ReleaseComObject(filters[0]);
        }
        Marshal.ReleaseComObject(enumFilters);
    }

    #endregion

    #region Rendering Methods

    public void RenderMedia(string filePath) {
        // 這裡實現如何根據 filePath 渲染媒體文件
        // 可調用 RenderMediaFilePrimary(filePath) 與 RenderMediaFileSecondary(filePath)

        // 渲染媒體文件
        RenderMediaFilePrimary(filePath);
        RenderMediaFileSecondary(filePath);
    }

    public void RenderMediaFilePrimary(string filePath) {
        int hr;

        try {
            IBaseFilter sourceFilter;
            hr = GraphBuilderPrimary.AddSourceFilter(filePath, "Source", out sourceFilter);
            DsError.ThrowExceptionForHR(hr);
            VideoWindowPrimary = (IVideoWindow) VideoRendererPrimary;
            VideoWindowPrimary.put_Visible(OABool.False);
            hr = ConnectFilters(GraphBuilderPrimary, sourceFilter, "Output", LavSplitterPrimary, "Input");
            DsError.ThrowExceptionForHR(hr);
            hr = ConnectFilters(GraphBuilderPrimary, LavSplitterPrimary, "Video", LavVideoDecoderPrimary, "Input");
            DsError.ThrowExceptionForHR(hr);
            hr = ConnectFilters(GraphBuilderPrimary, LavVideoDecoderPrimary, "Output", VideoRendererPrimary, "VMR Input0");
            DsError.ThrowExceptionForHR(hr);
            VideoWindowPrimary = (IVideoWindow) VideoRendererPrimary;
            VideoWindowPrimary.put_Owner(PrimaryForm.Instance.primaryScreenPanel.Handle); // 设置为 primaryScreenPanel 的句柄
            VideoWindowPrimary.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
            VideoWindowPrimary.SetWindowPosition(0, 0, 1500, 1000); // 调整视频窗口大小以填满黑色区域
            Task.Delay(100).Wait();
            VideoWindowPrimary.put_Visible(OABool.True);
            SaveGraphFile(GraphBuilderPrimary, "primary_graph.grf");

            if (hr == 0) {
                Console.WriteLine("主檔案 成功");
            } else {
                Console.WriteLine("檔案失敗");
            }
        } catch (Exception ex) {
            Console.WriteLine("主檔案失敗2: " + ex.Message);
        }
    }

    public void RenderMediaFileSecondary(string filePath) {
        int hr = GraphBuilderSecondary.RenderFile(filePath, null);
        DsError.ThrowExceptionForHR(hr);
        SaveGraphFile(GraphBuilderSecondary, "secondary_graph.grf");
        if (hr == 0) {
            Console.WriteLine("Secondary File rendered successfully.");
            SetAudioTrackTo(1);
        } else {
            Console.WriteLine("Failed to render secondary file.");
        }
    }

    private int ConnectFilters(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, string sourcePinName, IBaseFilter destFilter, string destPinName) {
        IPin outPin = FindPin(sourceFilter, sourcePinName);
        IPin inPin = FindPin(destFilter, destPinName);
        if (outPin == null || inPin == null) {
            Console.WriteLine(String.Format("Cannot find pins: {0} or {1}", sourcePinName, destPinName));
            return -1;
        }
        int hr = graphBuilder.Connect(outPin, inPin);
        return hr;
    }

    public void SetVolume(int volume) {
        if (AudioRenderer != null) {
            IBasicAudio basicAudio = (IBasicAudio)AudioRenderer;
            if (basicAudio != null) {
                basicAudio.put_Volume(volume);
            }
        }
    }
    public int GetVolume() {
        if (AudioRenderer != null) {
            IBasicAudio basicAudio = (IBasicAudio)AudioRenderer;
            if (basicAudio != null) {
                int volume;
                basicAudio.get_Volume(out volume);
                return volume;
            }
        }
        return -10000;
    }

    private bool isVocalRemoved = false;
    public async void ToggleVocalRemoval() {
        try {
            IAMStreamSelect streamSelect = (IAMStreamSelect)LavSplitterSecondary;

            if (streamSelect != null) {
                int trackCount;
                if (streamSelect.Count(out trackCount) == 0 && trackCount > 0) {
                    int currentTrackIndex = -1;
                    int audioTrack1 = -1;
                    int audioTrack2 = -1;

                    for (int i = 0; i < trackCount; i++) {
                        // 獲取音軌信息
                        AMMediaType mediaType;
                        AMStreamSelectInfoFlags flags;
                        int lcid, dwGroup;
                        string name;
                        object pObject, pUnk;

                        streamSelect.Info(i, out mediaType, out flags, out lcid, out dwGroup, out name, out pObject, out pUnk);

                        if (mediaType.majorType == MediaType.Audio) {
                            if (audioTrack1 == -1) {
                                audioTrack1 = i;
                            } else if (audioTrack2 == -1) {
                                audioTrack2 = i;
                            }

                            if ((flags & AMStreamSelectInfoFlags.Enabled) != 0) {
                                currentTrackIndex = i;
                            }
                        }

                        DsUtils.FreeAMMediaType(mediaType);
                    }

                    // 切換音軌
                    if (currentTrackIndex == audioTrack1 && audioTrack2 != -1) {
                        streamSelect.Enable(audioTrack2, AMStreamSelectEnableFlags.Enable);
                        isVocalRemoved = true;
                    } else if (currentTrackIndex == audioTrack2 && audioTrack1 != -1) {
                        streamSelect.Enable(audioTrack1, AMStreamSelectEnableFlags.Enable);
                        isVocalRemoved = false;
                    }
                    //OverlayForm.MainForm.ShowOriginalSongLabel();
                    string labelText = isVocalRemoved ? "無人聲" : "有人聲";
                    // 显示标签
                    OverlayForm.MainForm.ShowOriginalSongLabel(labelText);
                    await Task.Delay(300);
                    // 隐藏标签
                    OverlayForm.MainForm.HideOriginalSongLabel();
                }
            }
        } catch {}
    }

    public void SetAudioTrackTo(int trackIndex) {
        try {
            IAMStreamSelect streamSelect = (IAMStreamSelect)LavSplitterSecondary;

            if (streamSelect != null) {
                int trackCount;
                if (streamSelect.Count(out trackCount) == 0 && trackCount > 0) {
                    int audioTrackIndex = -1;

                    for (int i = 0; i < trackCount; i++) {
                        AMMediaType mediaType;
                        AMStreamSelectInfoFlags flags;
                        int lcid, dwGroup;
                        string name;
                        object pObject, pUnk;

                        streamSelect.Info(i, out mediaType, out flags, out lcid, out dwGroup, out name, out pObject, out pUnk);

                        if (mediaType.majorType == MediaType.Audio) {
                            audioTrackIndex++;
                            if (audioTrackIndex == trackIndex) {
                                streamSelect.Enable(i, AMStreamSelectEnableFlags.Enable);
                            } else {
                                streamSelect.Enable(i, AMStreamSelectEnableFlags.DisableAll);
                            }
                        }

                        DsUtils.FreeAMMediaType(mediaType);
                    }
                } else {}
            } else {}
        } catch {}
    }

    public static void SaveGraphFile(IGraphBuilder graph, string filename) {
        var writer = new StreamWriter(filename);
        IFilterGraph2 graph2 = (IFilterGraph2)graph;

        if (graph2 != null) {
            IEnumFilters enumFilters;
            graph2.EnumFilters(out enumFilters);

            enumFilters.Reset();
            IBaseFilter[] filters = new IBaseFilter[1];
            while (enumFilters.Next(1, filters, IntPtr.Zero) == 0) {
                FilterInfo filterInfo;
                filters[0].QueryFilterInfo(out filterInfo);
                writer.WriteLine("Filter: " + filterInfo.achName);
                IEnumPins enumPins;
                filters[0].EnumPins(out enumPins);
                enumPins.Reset();
                IPin[] pins = new IPin[1];
                while (enumPins.Next(1, pins, IntPtr.Zero) == 0) {
                    PinInfo pinInfo;
                    pins[0].QueryPinInfo(out pinInfo);
                    writer.WriteLine("  Pin: " + pinInfo.name);
                    Marshal.ReleaseComObject(pins[0]);
                }
                Marshal.ReleaseComObject(enumPins);
                Marshal.ReleaseComObject(filters[0]);
            }

            Marshal.ReleaseComObject(enumFilters);
        }

        writer.Close();
    }

    #endregion

    #region Playback Control and Resource Management
    
    public void InitializeAndPlayMedia(string filePath)
    {
        // 此方法主要由上層（如 VideoPlayerForm）呼叫，配置完畢後再做窗口綁定等操作
        if (VideoWindowPrimary != null)
            VideoWindowPrimary.put_Visible(OABool.False);
        if (VideoWindowSecondary != null)
            VideoWindowSecondary.put_Visible(OABool.False);
        
        RemoveAllFilters(GraphBuilderPrimary);
        RemoveAllFilters(GraphBuilderSecondary);

        InitializeGraphBuilders();
        RenderMedia(filePath);
    }

    public async Task ConfigureSecondaryVideoWindow(IntPtr ownerHandle, int width, int height)
    {
        VideoWindowSecondary = (IVideoWindow) VideoRendererSecondary;

        // 假設 VideoWindowSecondary 已經由其他流程初始化完成
        if (VideoWindowSecondary != null)
        {
            VideoWindowSecondary.put_Owner(ownerHandle);
            VideoWindowSecondary.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings);
            VideoWindowSecondary.SetWindowPosition(0, 0, width, height);
            // 給予短暫延遲，確保窗口配置完成
            await Task.Delay(100);
            VideoWindowSecondary.put_Visible(OABool.True);
        }
    }
    
    public void Play() {
        // 調用 mediaControl.Run()
        if (MediaControlPrimary != null)
            MediaControlPrimary.Run();
        if (MediaControlSecondary != null)
            MediaControlSecondary.Run();
        IsPaused = false;
        OverlayForm.MainForm.HidePauseLabel();
    }
    
    public void Pause() {
        // 調用 mediaControl.Pause()
        if (MediaControlPrimary != null)
            MediaControlPrimary.Pause();
        if (MediaControlSecondary != null)
            MediaControlSecondary.Pause();
        IsPaused = true;
        OverlayForm.MainForm.ShowPauseLabel();
    }

    public void PauseOrResumeSong() {
        if (IsPaused) {
            Play();
            PrimaryForm.Instance.pauseButton.Visible = true;
            PrimaryForm.Instance.playButton.Visible = false;
            PrimaryForm.Instance.syncPauseButton.Visible = true;
            PrimaryForm.Instance.syncPlayButton.Visible = false;
        } else {
            Pause();
            PrimaryForm.Instance.pauseButton.Visible = false;
            PrimaryForm.Instance.playButton.Visible = true;
            PrimaryForm.Instance.syncPauseButton.Visible = false;
            PrimaryForm.Instance.syncPlayButton.Visible = true;
            OverlayForm.MainForm.ShowPauseLabel();
        }
    }
    
    public void Stop() {
        MediaControlPrimary?.Stop();
        MediaControlSecondary?.Stop();
    }
    
    // 添加移除過濾器、釋放資源等輔助方法

    public void StopAndReleaseResources() {
        try {
            if (MediaControlPrimary != null) {
                MediaControlPrimary.Stop();
                Marshal.ReleaseComObject(MediaControlPrimary);
                MediaControlPrimary = null!;
            }
            if (MediaControlSecondary != null) {
                MediaControlSecondary.Stop();
                Marshal.ReleaseComObject(MediaControlSecondary);
                MediaControlSecondary = null!;
            }

            // 释放其他资源
            if (LavSplitterPrimary != null) {
                Marshal.ReleaseComObject(LavSplitterPrimary);
                LavSplitterPrimary = null!;
            }
            if (LavSplitterSecondary != null) {
                Marshal.ReleaseComObject(LavSplitterSecondary);
                LavSplitterSecondary = null!;
            }
            if (LavVideoDecoderPrimary != null) {
                Marshal.ReleaseComObject(LavVideoDecoderPrimary);
                LavVideoDecoderPrimary = null!;
            }
            if (LavVideoDecoderSecondary != null) {
                Marshal.ReleaseComObject(LavVideoDecoderSecondary);
                LavVideoDecoderSecondary = null!;
            }
            if (LavAudioDecoderSecondary != null) {
                Marshal.ReleaseComObject(LavAudioDecoderSecondary);
                LavAudioDecoderSecondary = null!;
            }
            if (OutputPinSecondary != null) {
                Marshal.ReleaseComObject(OutputPinSecondary);
                OutputPinSecondary = null!;
            }

            // 强制进行垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
        } catch (Exception ex) {
            Console.WriteLine($"釋放資源時發生錯誤: {ex.Message}");
        }
    }

    #endregion
}