namespace EightBitCalc;

class Processor
{
    // Class references
    private BinaryCalc binaryCalc;
    private Logger logger;

    // Set vars
    public byte[] registers { get; private set; } = new byte[BinaryCalc.REGISTER_NUM];
    public byte[] registersPrev { get; private set; } = new byte[BinaryCalc.REGISTER_NUM];
    public byte registerIndex { set; get; } = 0;
    public bool carryFlag { set; get; } = false;
    public bool zeroFlag { set; get; } = false;
    public bool negativeFlag { set; get; } = false;
    public bool overflowFlag { set; get; } = false;



    ///////////////////////////////////////////////////////////////////////////////
    // PROCESSOR METHODS:
    ///////////////////////////////////////////////////////////////////////////////

    // Change active register
    public void ChangeRegister(byte targetRegister)
    {
        registerIndex = targetRegister;
    }

    // Unset flags
    private void UnsetFlags()
    {
        zeroFlag = false;
        negativeFlag = false;
        overflowFlag = false;
    }

    // Set zero flag if value is zero
    private void SetZeroFlag()
    {
        if (registers[registerIndex] == 0b00000000) { zeroFlag = true; }
    }

    // Set negative flag if bit 7 is set
    private void SetNegativeFlag()
    {
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { negativeFlag = true; }
    }



    ///////////////////////////////////////////////////////////////////////////////
    // OPERATIONS:
    ///////////////////////////////////////////////////////////////////////////////

    // Set the (C)arry flag to 0
    public void CLC(bool silent = false)
    {
        carryFlag = false;
        if (!silent) { logger.AddMessage("CLC: CLEAR CARRY FLAG"); }
        logger.AddAssembly("CLC");
    }

    // Set the (C)arry flag to 1
    public void SEC(bool silent = false)
    {
        carryFlag = true;
        if (!silent) { logger.AddMessage("SEC: SET CARRY FLAG"); }
        logger.AddAssembly("SEC");
    }

    // Set the o(V)erflow flag to 0
    public void CLV(bool silent = false)
    {
        overflowFlag = false;
        if (!silent) { logger.AddMessage("CLV: CLEAR OVERFLOW FLAG"); }
        logger.AddAssembly("CLV");
    }

    // Logical AND operation
    public void AND(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] & bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "AND: Logical AND               (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("AND " + asm);
    }

    // Logical inclusive OR operation
    public void ORA(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] | bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "ORA: Logical Inclusive OR      (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("ORA " + asm);
    }

    // Exclusive OR operation
    public void EOR(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] ^ bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "EOR: Exclusive OR              (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("EOR " + asm);
    }

    // Load A operation
    public void LDA(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "LDA: Load Accumulator          (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("LDA " + asm);
    }

    // Load X operation
    public void LDX(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(1) + "] " + "LDX: Load X Register           (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("LDX " + asm);
    }

    // Load Y operation
    public void LDY(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false)
    {
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(2) + "] " + "LDY: Load Y Register           (" + BinaryCalc.ByteToString(bits) + ")"); }
        logger.AddAssembly("LDY " + asm);
    }

    // Store A operation
    public void STA(byte position, bool silent = false)
    {
        binaryCalc.memory[position] = registers[0];
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "STA: Store Accumulator         (" + BinaryCalc.ByteToString(binaryCalc.memory[position]) + ")"); }
        logger.AddAssembly("STA $" + BinaryCalc.HexToString(position));
    }

    // Store X operation
    public void STX(byte position, bool silent = false)
    {
        binaryCalc.memory[position] = registers[1];
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(1) + "] " + "STX: Store X Register          (" + BinaryCalc.ByteToString(binaryCalc.memory[position]) + ")"); }
        logger.AddAssembly("STX $" + BinaryCalc.HexToString(position));
    }

    // Store Y operation
    public void STY(byte position, bool silent = false)
    {
        binaryCalc.memory[position] = registers[2];
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(2) + "] " + "STY: Store Y Register          (" + BinaryCalc.ByteToString(binaryCalc.memory[position]) + ")"); }
        logger.AddAssembly("STY $" + BinaryCalc.HexToString(position));
    }

    // Transfer A to X operation
    public void TAX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] = registers[0];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "TAX: Transfer A to X           (" + BinaryCalc.ByteToString(registers[0]) + ")"); }
        logger.AddAssembly("TAX");
    }

    // Transfer A to Y operation
    public void TAY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] = registers[0];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "TAY: Transfer A to Y           (" + BinaryCalc.ByteToString(registers[0]) + ")"); }
        logger.AddAssembly("TAY");
    }

    // Transfer X to A operation
    public void TXA(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = registers[1];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(1) + "] " + "TXA: Transfer X to A           (" + BinaryCalc.ByteToString(registers[1]) + ")"); }
        logger.AddAssembly("TXA");
    }

    // Transfer Y to A operation
    public void TYA(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = registers[2];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(2) + "] " + "TYA: Transfer Y to A           (" + BinaryCalc.ByteToString(registers[2]) + ")"); }
        logger.AddAssembly("TYA");
    }

    // Arithmetic shift left operation
    public void ASL(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if ((registers[0] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[0] = (byte)(registers[0] << 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "ASL: Arithmetic shift left"); }
        logger.AddAssembly("ASL");
    }

    // Logical shift right operation
    public void LSR(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if ((registers[0] & 0b00000001) == 0b00000001) { carryFlag = true; }
        registers[0] = (byte)(registers[0] >> 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "LSR: Logical shift right"); }
        logger.AddAssembly("LSR");
    }

    // Rotate left operation
    public void ROL(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        byte extraBit = 0b00000000;
        if ((registers[0] & 0b10000000) == 0b10000000) { carryFlag = true; extraBit = 0b00000001; }
        registers[0] = (byte)(registers[0] << 1);
        registers[0] += extraBit;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "ROL: Rotate left"); }
        logger.AddAssembly("ROL");
    }

    // Rotate right operation
    public void ROR(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        byte extraBit = 0b00000000;
        if ((registers[0] & 0b00000001) == 0b00000001) { carryFlag = true; extraBit = 0b10000000; }
        registers[0] = (byte)(registers[0] >> 1);
        registers[0] += extraBit;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "ROR: Rotate right"); }
        logger.AddAssembly("ROR");
    }

    // Decrement X operation
    public void DEX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(1) + "] " + "DEX: Decrement X"); }
        logger.AddAssembly("DEX");
    }

    // Decrement Y operation
    public void DEY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(2) + "] " + "DEY: Decrement Y"); }
        logger.AddAssembly("DEY");
    }

    // Increment X operation
    public void INX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(1) + "] " + "INX: Increment X"); }
        logger.AddAssembly("INX");
    }

    // Increment Y operation
    public void INY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(2) + "] " + "INY: Increment Y"); }
        logger.AddAssembly("INY");
    }

    // Add with carry operation
    public void ADC(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false, bool inc = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (binaryCalc.settings["flagsAutoCarry"].enabled && carryFlag) { CLC(); }
        if (carryFlag && inc) { bits = 0b00000000; }
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000001 : 0b00000000));
        if (((registers[0] & 0b10000000) == 0b10000000) && (((registers[0] + bitsWithCarry) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] + bitsWithCarry) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] += bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "ADC: Add with carry            (" + BinaryCalc.ByteToString(bitsWithCarry) + ")"); }
        logger.AddAssembly("ADC " + asm);
    }

    // Subtract with carry operation
    public void SBC(byte bits, BinaryCalc.BYTE_TYPE type = BinaryCalc.BYTE_TYPE.BINARY, bool silent = false, bool dec = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (binaryCalc.settings["flagsAutoCarry"].enabled && !carryFlag) { SEC(); }
        if (!carryFlag && dec) { bits = 0b00000000; }
        binaryCalc.FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000000 : 0b00000001));
        if (registers[0] - bitsWithCarry < 0) { carryFlag = false; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] - bitsWithCarry) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] -= bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { logger.AddMessage("[" + BinaryCalc.GetRegisterChar(0) + "] " + "SBC: Subtract with carry       (" + BinaryCalc.ByteToString(bitsWithCarry) + ")"); }
        logger.AddAssembly("SBC " + asm);
    }

    // Constructor
    public Processor(BinaryCalc binaryCalc, Logger logger) 
    {
        this.binaryCalc = binaryCalc;
        this.logger = logger;
    }
}