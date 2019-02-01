# BattleTech MadLibs 1.4 Fix
The HBS game [BattleTech](http://www.battletechgame.com/) includes a string interpolation function known as MadLibs. This function is slightly broken in the 1.4 release, which causes the interpolation to not happen in some edge cases. This patch corrects this issue and allows the interpolation to proceed as expected.

Please note that is a **hack** crafted to work only until HBS corrects this issue in a future patch or hotfix. You should NOT run this on BT 1.3.2 or below, and you should NOT run this on a future version once this behavior has been corrected.

The implementation is largely from HBS_Eck, who provided the raw code - I simply ported this to Harmony patches. Thanks for HBS for a great game and for a quick response!

