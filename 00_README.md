# Predictive Aiming in Unity Integration testing project

This project was created to work through some minor problems in predictive aiming code from [Kain Shin's great article](http://www.gamasutra.com/blogs/KainShin/20090515/83954/Predictive_Aim_Mathematics_for_AI_Targeting.php).  Specifically, the predictive aim will fail in some circumstances including:

 - a NaN result when the target's speed is greater than the bullet's speed.
 - a Inf result when the target's speed equals the bullet's speed.

From an intuitive point of view, both of these cases *can* have valid shooting solutions in some cases.

## Instructions

 - Clone project locally
 - Open with Unity3D.
 - Open scene "01UGS/01Scenes/PreemptiveAimingTest01.unity".
 - Open Menu "Unity Test Tools | Integration Test Runner".
 - On new window, press "Run All".

## Current expectation

There are 2 groups of tests that will pass and 1 group that currently fail:

 - Pass:
  - 01 Motionless tests
  - 02 Moving tests

 - Fail:
  - 03 Fast Moving tests
    *(although "03 Moving + ShotNoGrav + aimFaked" will pass and be ignored -- it is there to show a valid solution exists despite algorithm being unable to find it.)*

----

N.b. If you're not familiar with "Unity Test Tools", you can change whether interaction is allowed during tests and toggle "pause on failures" from the icon in the top-right corner of the "Integration Tests" window (down-arrow + 3 horizontal lines).

