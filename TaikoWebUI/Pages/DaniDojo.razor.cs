﻿namespace TaikoWebUI.Pages;

public partial class DaniDojo
{
    [Parameter]
    public int Baid { get; set; }

    private DanBestDataResponse? response;

    private Dictionary<uint, DanBestData> bestDataMap = new();

    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards"),
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        response.ThrowIfNull();
        response.DanBestDataList.ForEach(data => data.DanBestStageDataList
            .Sort((stageData, otherStageData) => stageData.SongNumber.CompareTo(otherStageData.SongNumber)));
        bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);

        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem("Dani Dojo", href: $"/Cards/{Baid}/DaniDojo", disabled: false));
    }

    private static string GetDanRequirementTitle(DanData.OdaiBorder data)
    {
        var danConditionType = (DanConditionType)data.OdaiType;
        return (DanBorderType)data.BorderType switch
        {
            DanBorderType.All => $"{danConditionType}, All song",
            DanBorderType.PerSong => $"{danConditionType}, Per song",
            _ => throw new ArgumentOutOfRangeException(nameof(data))
        };
    }

    private static string GetDanRequirementAll(DanData.OdaiBorder odaiBorder)
    {
        ((DanBorderType)odaiBorder.BorderType).Throw().IfNotEquals(DanBorderType.All);
        var type = (DanConditionType)odaiBorder.OdaiType;
        var template =
            $"Pass when {{0}} {{1}} {odaiBorder.RedBorderTotal}, gold when {{1}} {odaiBorder.GoldBorderTotal}";
        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => string.Format(template, "good count", "exceeds"),
            DanConditionType.OkCount => string.Format(template, "ok count", "exceeds"),
            DanConditionType.BadCount => string.Format(template, "bad count", "below"),
            DanConditionType.ComboCount => string.Format(template, "combo count", "exceeds"),
            DanConditionType.DrumrollCount => string.Format(template, "drum roll count", "exceeds"),
            DanConditionType.Score => string.Format(template, "score", "exceeds"),
            DanConditionType.TotalHitCount => string.Format(template, "hit count", "exceeds"),
            _ => throw new ArgumentOutOfRangeException(nameof(odaiBorder))
        };
    }

    private static string GetDanRequirementPerSong(DanData.OdaiBorder odaiBorder, int songNumber)
    {
        songNumber.Throw().IfOutOfRange(0, 2);
        ((DanBorderType)odaiBorder.BorderType).Throw().IfNotEquals(DanBorderType.PerSong);
        var type = (DanConditionType)odaiBorder.OdaiType;
        var template =
            $"Pass when song{songNumber}'s {{0}} {{1}} {odaiBorder.RedBorderTotal}, gold when {{1}} {odaiBorder.GoldBorderTotal}";
        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => string.Format(template, "good count", "exceeds"),
            DanConditionType.OkCount => string.Format(template, "ok count", "exceeds"),
            DanConditionType.BadCount => string.Format(template, "bad count", "below"),
            DanConditionType.ComboCount => string.Format(template, "combo count", "exceeds"),
            DanConditionType.DrumrollCount => string.Format(template, "drum roll count", "exceeds"),
            DanConditionType.Score => string.Format(template, "score", "exceeds"),
            DanConditionType.TotalHitCount => string.Format(template, "hit count", "exceeds"),
            _ => throw new ArgumentOutOfRangeException(nameof(odaiBorder))
        };
    }

    private static string GetAllBestFromData(DanConditionType type, DanBestData data)
    {
        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => data.DanBestStageDataList.Sum(stageData => stageData.GoodCount).ToString(),
            DanConditionType.OkCount => data.DanBestStageDataList.Sum(stageData => stageData.OkCount).ToString(),
            DanConditionType.BadCount => data.DanBestStageDataList.Sum(stageData => stageData.BadCount).ToString(),
            DanConditionType.ComboCount => data.ComboCountTotal.ToString(),
            DanConditionType.DrumrollCount => data.DanBestStageDataList.Sum(stageData => stageData.DrumrollCount)
                .ToString(),
            DanConditionType.Score => data.DanBestStageDataList.Sum(stageData => stageData.PlayScore).ToString(),
            DanConditionType.TotalHitCount => data.DanBestStageDataList.Sum(stageData => stageData.TotalHitCount)
                .ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static string GetSongBestFromData(DanConditionType type, DanBestData data, int songNumber)
    {
        songNumber.Throw().IfOutOfRange(0, 2);
        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => data.DanBestStageDataList[songNumber].GoodCount.ToString(),
            DanConditionType.OkCount => data.DanBestStageDataList[songNumber].OkCount.ToString(),
            DanConditionType.BadCount => data.DanBestStageDataList[songNumber].BadCount.ToString(),
            DanConditionType.ComboCount => data.DanBestStageDataList[songNumber].ComboCount.ToString(),
            DanConditionType.DrumrollCount => data.DanBestStageDataList[songNumber].DrumrollCount.ToString(),
            DanConditionType.Score => data.DanBestStageDataList[songNumber].PlayScore.ToString(),
            DanConditionType.TotalHitCount => data.DanBestStageDataList[songNumber].TotalHitCount.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static string GetDanTitle(string title)
    {
        return title switch
        {
            "5kyuu" => "Fifth Kyuu",
            "4kyuu" => "Fourth Kyuu",
            "3kyuu" => "Third Kyuu",
            "2kyuu" => "Second Kyuu",
            "1kyuu" => "First Kyuu",
            "1dan"  => "First Dan",
            "2dan"  => "Second Dan",
            "3dan"  => "Third Dan",
            "4dan"  => "Fourth Dan",
            "5dan"  => "Fifth Dan",
            "6dan"  => "Sixth Dan",
            "7dan"  => "Seventh Dan",
            "8dan"  => "Eighth Dan",
            "9dan"  => "Ninth Dan",
            "10dan" => "Tenth Dan",
            "11dan" => "Kuroto",
            "12dan" => "Meijin",
            "13dan" => "Chojin",
            "14dan" => "Tatsujin",
            "15dan" => "Gaiden",
            _ => ""
        };
    }

    private string GetDanResultIcon(uint danId)
    {
        var icon = "";

        if (!bestDataMap.ContainsKey(danId))
        {
            return icon;
        }
        
        var state = bestDataMap[danId].ClearState;
            
        if (state is not DanClearState.NotClear)
        {
            icon = $"<image href='/images/dani_{state}.png' width='24' height='24'/>";
        }

        return icon;
    }

    private static string GetSoulGauge(DanData data, bool isGold)
    {
        var borders = data.OdaiBorderList;
        var soulBorder =
            borders.FirstOrDefault(border => (DanConditionType)border.BorderType == DanConditionType.SoulGauge,
                new DanData.OdaiBorder());

        if (isGold)
        {
            return $"{soulBorder.GoldBorderTotal}%";
        }

        return $"{soulBorder.RedBorderTotal}%";
    }
}