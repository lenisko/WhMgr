﻿namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Extensions;

    [
        Group("settings"),
        Aliases("config", "cfg", "conf", "c"),
        Description("Event Pokemon management commands."),
        Hidden,
        RequirePermissions(Permissions.KickMembers)
    ]
    public class Settings : BaseCommandModule
    {
        private readonly WhConfigHolder _whConfig;

        public Settings(WhConfigHolder whConfig)
        {
            _whConfig = whConfig;
        }

        [
            Command("set"),
            Aliases("s"),
            Description("")
        ]
        public async Task SetAsync(CommandContext ctx,
            [Description("")] string key,
            [Description("")] string value)
        {
            // TODO: Provide list of available config options to set.
            if (!await ctx.IsDirectMessageSupported(_whConfig.Instance))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

            if (!_whConfig.Instance.Servers.ContainsKey(guildId))
            {
                // TODO: Localize
                await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({guildId}) not configured in {Strings.ConfigFileName}");
                return;
            }

            //var guildConfig = _whConfig.Instance.Servers[guildId];
            switch (key)
            {
                case "nest_channel":
                    // TODO: Validate nestChannelId
                    //_whConfig.Instance.Servers[guildId].NestsChannelId = value;
                    //_whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "prefix":
                    var oldPrefix = _whConfig.Instance.Servers[guildId].CommandPrefix;
                    await ctx.RespondEmbed($"{ctx.User.Username} Command prefix changed from {oldPrefix} to {value}.", DiscordColor.Green);
                    _whConfig.Instance.Servers[guildId].CommandPrefix = value;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "enable_cities":
                    if (!bool.TryParse(value, out var enableCities))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].EnableCities = enableCities;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "enable_subscriptions":
                    if (!bool.TryParse(value, out var enableSubscriptions))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].Subscriptions.Enabled = enableSubscriptions;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "cities_require_donor":
                    if (!bool.TryParse(value, out var citiesRequireDonor))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].CitiesRequireSupporterRole = citiesRequireDonor;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "prune_quests":
                    if (!bool.TryParse(value, out var pruneQuests))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].PruneQuestChannels = pruneQuests;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "icon_style":
                    if (!_whConfig.Instance.IconStyles.ContainsKey(value))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].IconStyle = value;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
                case "shiny_stats":
                    if (!bool.TryParse(value, out var enableShinyStats))
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}", DiscordColor.Red);
                        return;
                    }
                    _whConfig.Instance.Servers[guildId].ShinyStats.Enabled = enableShinyStats;
                    _whConfig.Instance.Save(_whConfig.Instance.FileName);
                    break;
            }
            await Task.CompletedTask;
        }

        [
            Command("list"),
            Aliases("l"),
            Description("List config settings for current guild.")
        ]
        public async Task ListSettingsAsync(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_whConfig.Instance))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

            if (!_whConfig.Instance.Servers.ContainsKey(ctx.Guild?.Id ?? 0))
            {
                // TODO: Localize
                await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({guildId}) not configured in {Strings.ConfigFileName}");
                return;
            }

            var guildConfig = _whConfig.Instance.Servers[guildId];
            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blurple,
                Title = $"{ctx.Guild.Name} Config",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };

            // TODO: Localize
            eb.AddField($"Enable Cities", guildConfig.EnableCities ? "Yes" : "No", true);
            eb.AddField($"City Roles", string.Join("\r\n", guildConfig.CityRoles), true);
            eb.AddField($"Enable Subscriptions", guildConfig.Subscriptions.Enabled ? "Yes" : "No", true);
            eb.AddField($"Command Prefix", guildConfig.CommandPrefix ?? "@BotMentionHere", true);
            eb.AddField($"City Roles Require Donor Role", guildConfig.CitiesRequireSupporterRole ? "Yes" : "No", true);
            eb.AddField($"Donor Roles", string.Join("\r\n", guildConfig.DonorRoleIds.Select(x => $"{ctx.Guild.GetRole(x).Name}:{x}")), true);
            // TODO: Use await
            //eb.AddField($"Moderators", string.Join("\r\n", guildConfig.ModeratorRoleIds.Select(x => $"{ctx.Client.GetMemberById(guildId, x).GetAwaiter().GetResult().Username}:{x}")), true);
            eb.AddField($"Nest Channel", guildConfig.NestsChannelId == 0 ? "Not Set" : $"{ctx.Guild.GetChannel(guildConfig.NestsChannelId)?.Name}:{guildConfig.NestsChannelId}", true);
            eb.AddField($"Prune Quest Channels", guildConfig.PruneQuestChannels ? "Yes" : "No", true);
            eb.AddField($"Quest Channels", string.Join("\r\n", guildConfig.QuestChannelIds.Select(x => $"{ctx.Guild.GetChannel(x)?.Name}:{x}")), true);
            eb.AddField($"Enable Shiny Stats", guildConfig.ShinyStats?.Enabled ?? false ? "Yes" : "No", true);
            eb.AddField($"Shiny Stats Channel", guildConfig.ShinyStats?.ChannelId == 0 ? "Not Set" : $"{ctx.Guild.GetChannel(guildConfig.ShinyStats.ChannelId)?.Name}:{guildConfig.ShinyStats?.ChannelId}", true);
            eb.AddField($"Clear Previous Shiny Stats", guildConfig.ShinyStats?.ClearMessages ?? false ? "Yes" : "No", true);
            eb.AddField($"Icon Style", guildConfig.IconStyle, true);
            await ctx.RespondAsync(embed: eb);
        }

        [
            Group("roles"),
            Aliases("cities"),
            Description("Event Pokemon management commands."),
            Hidden,
            RequirePermissions(Permissions.KickMembers)
        ]
        public class CityRoles
        {
            private readonly WhConfigHolder _whConfig;
            private readonly SubscriptionProcessor _subProcessor;

            public CityRoles(WhConfigHolder whConfig, SubscriptionProcessor subProcessor)
            {
                _whConfig = whConfig;
                _subProcessor = subProcessor;
            }

            [
                Command("list"),
                Aliases("l"),
                Description("")
            ]
            public async Task ListAsync(CommandContext ctx)
            {
                if (!await ctx.IsDirectMessageSupported(_whConfig.Instance))
                    return;

                var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

                if (!_whConfig.Instance.Servers.ContainsKey(guildId))
                {
                    // TODO: Localize
                    await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({guildId}) not configured in {Strings.ConfigFileName}", DiscordColor.Red);
                    return;
                }

                var guildConfig = _whConfig.Instance.Servers[guildId];
                var eb = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Blurple,
                    Title = $"{ctx.Guild.Name} Config - City Roles",
                    // TODO: Add EnabledCities/CitiesRequiresDonorRole to description
                    Description = $"- {string.Join("\r\n- ", guildConfig.CityRoles)}",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                        IconUrl = ctx.Guild?.IconUrl
                    }
                };
                await ctx.RespondAsync(embed: eb);
            }

            [
                Command("add"),
                Description("a")
            ]
            public async Task AddAsync(CommandContext ctx,
                [Description(""), RemainingText] string roleNames)
            {
                if (!await ctx.IsDirectMessageSupported(_whConfig.Instance))
                    return;

                var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

                if (!_whConfig.Instance.Servers.ContainsKey(guildId))
                {
                    // TODO: Localize
                    await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({guildId}) not configured in {Strings.ConfigFileName}", DiscordColor.Red);
                    return;
                }

                var guildConfig = _whConfig.Instance.Servers[guildId];

                var rolesAdded = new List<string>();
                var rolesFailed = new List<string>();
                var guildRoleNames = ctx.Guild.Roles.Select(x => x.Value.Name.ToLower()).ToList();
                var split = roleNames.Split(',');
                for (var i = 0; i < split.Length; i++)
                {
                    var roleName = split[i];
                    if (guildRoleNames.Contains(roleName) && guildConfig.CityRoles.Select(x => x.ToLower()).Contains(roleName.ToLower()))
                    {
                        rolesAdded.Add(roleName);
                        continue;
                    }

                    rolesFailed.Add(roleName);
                }

                _whConfig.Instance.Servers[guildId].CityRoles.AddRange(rolesAdded);
                _whConfig.Instance.Save(_whConfig.Instance.FileName);

                // TODO: Localize
                var message = $"{ctx.User.Username} Successfully added the following roles: {string.Join(", ", rolesAdded)}";
                if (rolesFailed.Count > 0)
                {
                    message += $"\r\n{ctx.User.Username} Failed to add the following roles: {string.Join(", ", rolesFailed)}";
                }
                await ctx.RespondEmbed(message);

                // TODO: Reload config
            }

            [
                Command("remove"),
                Aliases("rem", "rm", "r"),
                Description("")
            ]
            public async Task RemoveAsync(CommandContext ctx,
                [Description(""), RemainingText] string roleNames)
            {
                if (!await ctx.IsDirectMessageSupported(_whConfig.Instance))
                    return;

                var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

                if (!_whConfig.Instance.Servers.ContainsKey(guildId))
                {
                    // TODO: Localize
                    await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({guildId}) not configured in {Strings.ConfigFileName}", DiscordColor.Red);
                    return;
                }

                var guildConfig = _whConfig.Instance.Servers[guildId];

                var rolesRemoved = new List<string>();
                var rolesFailed = new List<string>();
                var split = roleNames.Split(',');
                for (var i = 0; i < split.Length; i++)
                {
                    var roleName = split[i];
                    if (guildConfig.CityRoles.Select(x => x.ToLower()).Contains(roleName.ToLower()))
                    {
                        rolesRemoved.Add(roleName);
                        continue;
                    }

                    rolesFailed.Add(roleName);
                }

                rolesRemoved.ForEach(x => _whConfig.Instance.Servers[guildId].CityRoles.Remove(x));
                _whConfig.Instance.Save(_whConfig.Instance.FileName);

                // TODO: Localize
                var message = $"{ctx.User.Username} Successfully removed the following roles: {string.Join(", ", rolesRemoved)}";
                if (rolesFailed.Count > 0)
                {
                    message += $"\r\n{ctx.User.Username} Failed to remove the following roles: {string.Join(", ", rolesFailed)}";
                }
                await ctx.RespondEmbed(message);

                // TODO: Reload config
            }
        }
    }
}
//List/add/remove quest channel pruning
//Manage shiny stats