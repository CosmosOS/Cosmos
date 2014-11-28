HMI.Init() needs to be called after Cosmos has finished booted.

example:  

  protected void BeforeRun()
  {
     Cosmos.Core.HMI.Init();
  }