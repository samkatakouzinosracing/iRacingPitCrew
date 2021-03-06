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
using iRacingPitCrew.PitCrewCommands;
using NUnit.Framework;
using System;

namespace iRacingPitCrew.Tests.PitCrewCommands
{
    [TestFixture]
    public class FuelStrategyCalculatorTest
    {
        [Test]
        public void ShouldCalculateTotalFuelRequiredFor20Laps()
        {
            var r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 1.9d, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(45)); //( 20+1)*1.9 => 39.9 => 40 => then to nearest 5

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 0.9d, fuelTankCapacity: 10);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(20)); //( 20+1)*0.9 => then to nearest 5

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2.4d, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(55)); //( 20+1)*2.4 => 50.4 => 51 then to nearest 5

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2.380952380952381d, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(55)); //( 20+1)*2.38...  => 50.0 then to nearest 5

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2.333333333333333, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(50));

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2.333333333333334, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(55));

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2.380952380952381d, fuelTankCapacity: 1);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(55));
        }

        [Test]
        public void ShouldCalculateNumerOfPitsStopsForA20LapRace()
        {
            var r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 2d, fuelTankCapacity: 30);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(1));

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 0.9d, fuelTankCapacity: 10);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(20)); //( 20+1)*0.9 => then to nearest 5
            Assert.That(r.NumberOfPitStops, Is.EqualTo(2));

            r = FuelStrategy.Calculate(numberOfRaceLaps: 20, averageFuelBurnPerLap: 0.9d, fuelTankCapacity: 100);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(0));
        }

        [Test]
        public void ShouldCalculateEstimateNumberOfRaceLapsFor20Minutes()
        {
            var r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 0f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.EstimatedNumberOfRaceLaps, Is.EqualTo(21));

            r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 0f, averageLapTime: 55.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.EstimatedNumberOfRaceLaps, Is.EqualTo(22));
        }

        [Test]
        public void ShouldCalculateTotalFuelRequiredForRaceDuration()
        {
            var r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 1f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(25)); //( 21+1)*1 =>21 then to nearest 5

            r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 1.9f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(45)); //( 21+1)*1.9 =>39 then to nearest 5

            r = FuelStrategy.Calculate(raceDuration: 40.Minutes(), averageFuelBurnPerLap: 1.9f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.TotalFuelRequired, Is.EqualTo(85));
        }

        [Test]
        public void ShouldCalculateNumerOfPitsStopsForARaceDuration()
        {
            var r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 1f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(0)); //Total fuel 25L => 0 Stop

            r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 1f, averageLapTime: 60.Seconds(), fuelTankCapacity: 25);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(0)); //Total fuel 25L => 0 Stop

            r = FuelStrategy.Calculate(raceDuration: 20.Minutes(), averageFuelBurnPerLap: 1.9f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(1)); //Total fuel 45L => 1 Stop

            r = FuelStrategy.Calculate(raceDuration: 40.Minutes(), averageFuelBurnPerLap: 1.9f, averageLapTime: 60.Seconds(), fuelTankCapacity: 30);
            Assert.That(r.NumberOfPitStops, Is.EqualTo(3)); //Total fuel 85L => 2 Stop (refill => 30 - 1.9*2 => 26.2 => 85/26.2
        }
    }
}
