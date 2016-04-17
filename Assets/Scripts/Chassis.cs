class Chassis
{
    public int MaxTicks;
    public bool Silent;
    public bool Destructive;

    public Chassis(int ticks, bool silent, bool destructive)
    {
        MaxTicks = ticks;
        Silent = silent;
        Destructive = destructive;
    }

    public Chassis(ChassisState state)
    {
        switch (state)
        {
            case ChassisState.BASIC:
                MaxTicks = 1;
                Silent = false;
                Destructive = false;
                break;
            case ChassisState.SILENT:
                MaxTicks = 1;
                Silent = true;
                Destructive = false;
                break;
            case ChassisState.FAST:
                MaxTicks = 3;
                Silent = false;
                Destructive = false;
                break;
            case ChassisState.OFFROAD:
                MaxTicks = 1;
                Silent = false;
                Destructive = true;
                break;
        }
    }
}