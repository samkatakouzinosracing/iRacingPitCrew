﻿// This file is part of iRacingPitCrew Application.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingPitCrew.Net
//
// iRacingPitCrew is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingPitCrew is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingPitCrew.  If not, see <http://www.gnu.org/licenses/>.

using iRacingSDK.Support;
using System;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace iRacingPitCrew.PitCrewCommands
{
    public class FuelStrategyCommand : PitCrewCommand
    {
        readonly DataCollector dataCollector;
        readonly Action conversation;

        public FuelStrategyCommand(SpeechRecognitionEngine recognizer, SpeechSynthesizer synthesizer, Action recognized, DataCollector dataCollector)
            : base(recognizer, synthesizer, recognized)
        {
            this.dataCollector = dataCollector;
            SetGrammar("fuel strategy");

            conversation = FromRaceLength(FromTankSize());
        }

        protected override void Command(RecognitionResult rr)
        {
//            dataCollector.AverageFuelPerLap = 1.9f;
//            dataCollector.AverageLapTimeSpan = 66.2.Seconds();

            if (dataCollector.AverageFuelPerLap <= 0 || dataCollector.AverageLapTimeSpan.TotalSeconds <= 0)
            {
                SpeakAsync("Do not have any fuel consumption or lap timing data.  You need to do 5 laps at least");
                return;
            }

            conversation();
        }

        Action FromRaceLength(Action next)
        {
            var question = AskQuestion(
                question: "What is your race length?",
                matching: g =>
                {
                    g.Append(new SemanticResultKey("amount", Numbers()));
                    g.Append(new Choices(new GrammarBuilder("laps"), new GrammarBuilder("minutes")));
                },
                answer: rr =>
                {
                    if (rr.Text.EndsWith("laps"))
                        dataCollector.RaceDuration = new RaceDuration((int)rr.Semantics["amount"].Value, RaceType.Laps);
                    else
                        dataCollector.RaceDuration = new RaceDuration((int)rr.Semantics["amount"].Value, RaceType.Minutes);
                },
                next: next
            );

            return () => {
                if (!dataCollector.RaceDuration.IsEmpty)
                    next();
                else
                    question();
            };
        }

        Action FromTankSize()
        {
            var question = AskQuestion(
                question: "What is your fuel tank capacity?",
                matching: g =>
                {
                    g.Append(new SemanticResultKey("amount", Numbers()));
                    g.Append(new Choices("litre", "litres", "liters", "liter"));
                },
                answer: rr =>
                {
                    dataCollector.TankSize = (int)rr.Semantics["amount"].Value;
                    Calculate();
                },
                next: () => { }
            );

            return () =>
            {
                if (dataCollector.TankSize != null)
                    Calculate();
                else
                    question();
            };
        }

        void Calculate()
        {
            if( dataCollector.RaceDuration.Type == RaceType.Laps)
                SpeakAsync("Your race length is {0} laps".F(dataCollector.RaceDuration.Length));
            else
                SpeakAsync("Your race length is {0} minutes".F(dataCollector.RaceDuration.Length));

            SpeakAsync("Your tank size is {0}".F(dataCollector.TankSize));

            var t = dataCollector.AverageLapTimeSpan;

            SpeakAsync("Your average fuel usage is {0:0.00} litres per lap.".F(dataCollector.AverageFuelPerLap));

            if (dataCollector.RaceDuration.Type == RaceType.Minutes)
            {
                var r = FuelStrategy.Calculate(dataCollector.RaceDuration.TotalMinutes, dataCollector.AverageFuelPerLap, dataCollector.AverageLapTimeSpan, (int)dataCollector.TankSize);

                SpeakAsync("Estimating you will do {0} laps in {1} minutes".F(r.EstimatedNumberOfRaceLaps, (int)r.RaceDuration.TotalMinutes));
                SpeakAsync("For a {0} minute race, you will need a total of {1} litres".F((int)r.RaceDuration.TotalMinutes, r.TotalFuelRequired));
                if (r.NumberOfPitStops == 0)
                    SpeakAsync("You will not need to stop");
                else
                    SpeakAsync("You will need to stop {0}".F(ToWords(r.NumberOfPitStops)));
            }
            else
            {
                var r = FuelStrategy.Calculate(dataCollector.RaceDuration.Length, dataCollector.AverageFuelPerLap, (int)dataCollector.TankSize);

                SpeakAsync("For a {0} lap race, you will need a total of {1} litres".F(dataCollector.RaceDuration.Length, r.TotalFuelRequired));
                if( r.NumberOfPitStops == 0 )
                    SpeakAsync("You will not need to stop");
                else
                SpeakAsync("You will need to stop {0}".F(ToWords(r.NumberOfPitStops)));
            }
        }

        private string ToWords(int p)
        {
            switch( p)
            {
                case 1:
                    return "once";

                case 2:
                    return "twice";
            }

            return "{0} times".F(p);
        }

    }
}
