namespace FishMatch.EditorTools
{
    // The one-off setup scripts under Assets/Editor build on each other's scene/asset output and are
    // only safe to run as a complete forward sequence - running one alone after an earlier one changes
    // can silently break things it depends on (e.g. re-running LevelSetup destroys and rebuilds
    // Match3Test's Canvas, orphaning references FishCollectionSetup wired into the old one).
    //
    // Required order: KenneyImportSetup -> UIShapeSetup -> FishMatchSetup -> LevelSetup ->
    // FishCollectionSetup -> AquariumSetup -> DecorationShopSetup -> FishDexSetup -> LevelMapSetup ->
    // MainMenuSetup. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.ProjectSetupRunner.RunAll -quit
    public static class ProjectSetupRunner
    {
        public static void RunAll()
        {
            KenneyImportSetup.Run();
            UIShapeSetup.Run();
            FishMatchSetup.Run();
            LevelSetup.Run();
            FishCollectionSetup.Run();
            AquariumSetup.Run();
            DecorationShopSetup.Run();
            FishDexSetup.Run();
            LevelMapSetup.Run();
            MainMenuSetup.Run();
        }
    }
}
