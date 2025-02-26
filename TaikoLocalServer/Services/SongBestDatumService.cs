﻿using SharedProject.Models;
using Swan.Mapping;
using TaikoLocalServer.Services.Interfaces;
using Throw;

namespace TaikoLocalServer.Services;

public class SongBestDatumService : ISongBestDatumService
{
    private readonly TaikoDbContext context;

    public SongBestDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<SongBestDatum>> GetAllSongBestData(uint baid)
    {
        return await context.SongBestData.Where(datum => datum.Baid == baid).ToListAsync();
    }

    public async Task UpdateOrInsertSongBestDatum(SongBestDatum datum)
    {
        var existing = await context.SongBestData.AnyAsync(
            bestDatum => bestDatum.Baid == datum.Baid &&
                         bestDatum.Difficulty == datum.Difficulty &&
                         bestDatum.SongId == datum.SongId);
        if (existing)
        {
            context.SongBestData.Update(datum);
            await context.SaveChangesAsync();
            return;
        }

        await context.SongBestData.AddAsync(datum);
        await context.SaveChangesAsync();
    }

    public async Task<List<SongBestData>> GetAllSongBestAsModel(uint baid)
    {
        var songbestDbData = await context.SongBestData.Where(datum => datum.Baid == baid)
            .ToListAsync();

        var result = songbestDbData.Select(datum => datum.CopyPropertiesToNew<SongBestData>()).ToList();

        var playLogs = await context.SongPlayData.Where(datum => datum.Baid == baid).ToListAsync();
        foreach (var bestData in result)
        {
            var songPlayDatums = playLogs.Where(datum => datum.Difficulty == bestData.Difficulty &&
                                                         datum.SongId == bestData.SongId).ToArray();
            songPlayDatums.Throw($"Play log for song id {bestData.SongId} is null! " +
                                 "Something is wrong with db!")
                .IfEmpty();
            var lastPlayLog = songPlayDatums
                .MaxBy(datum => datum.PlayTime);
            bestData.LastPlayTime = lastPlayLog!.PlayTime;
            
            var bestLog = songPlayDatums
                .MaxBy(datum => datum.Score);
            bestLog.CopyOnlyPropertiesTo(bestData,
                nameof(SongPlayDatum.GoodCount),
                nameof(SongPlayDatum.OkCount),
                nameof(SongPlayDatum.MissCount),
                nameof(SongPlayDatum.HitCount),
                nameof(SongPlayDatum.DrumrollCount),
                nameof(SongPlayDatum.ComboCount)
            );
        }

        return result;
    }
}