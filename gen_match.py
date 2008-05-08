#!/usr/bin/python
import fileinput
import string
import sys

# Takes CIL and generates Smokey code to recognize the instructions.
# A path to the CIL code can be passed into the script or stdin can
# be used. The Smokey code is written to stdout.
#
# Blank lines are ignored.
# Lines whose first non-whitespace character is '#' are also ignored.
# Non-blank lines are white-space separated: the offset apepars first
# (0A:), then the instruction (ble.s), then any arguments (1F).
#
# If the instruction starts with an upper case letter it is used as is
# (except periods are replaced with underscores). Otherwise a table
# lookup is done with the part before the first period so we can test
# for all the relevant variants of the instruction.
#
# If a stloc/ldloc ends with an upper-case letter (ldloc.N) then it will
# match any variant of the instruction and all instructions which use
# that letter must have equal operand values.
#
# Call and callvirt attempt to match the target strings. A '*' wild
# card can be used at the start and/or end of the target string. A '*'
# in the middle matches both the start and the end of the target string.

# comment lines and blank lines are allowed
# 03: callvirt   System.Int32 System.String::IndexOf(System.Char)
# 08: stloc.N    V_0		the load and the store must use the same variable
# 09: ldloc.N    V_0
# 0A: ldc.i4.0   
# 0B: ble        1F

def filterLine(line):
	filter = False
	
	if len(line) == 0:
		filter = True
		
	elif line[0] == '#':
		filter = True
		
	return filter

def findOpCode(line):
	i = line.find(" ")
	j = line.find(" ", i + 1)
	if j >= 0:
		return line[i+1:j]
	else:
		return line[i+1:]

ops = {
	"add"      : ["Add", "Add_Ovf", "Add_Ovf_Un"],
	"beq"      : ["Beq_S", "Beq"],
	"bge"      : ["Bge_S", "Bge_Un_S", "Bge", "Bge_Un"],
	"bgt"      : ["Bgt_S", "Bgt_Un_S", "Bgt", "Bgt_Un"],
	"ble"      : ["Ble_S", "Ble_Un_S", "Ble", "Ble_Un"],
	"blt"      : ["Blt_S", "Blt_Un_S", "Blt", "Blt_Un"],
	"bne"      : ["Bne_Un_S", "Bne_Un"],
	"br"       : ["Br_S", "Br"],
	"brfalse"  : ["Brfalse_S", "Brfalse"],
	"brtrue"   : ["Brtrue_S", "Brtrue"],
	"call"     : ["Call", "Callvirt"],
	"callvirt" : ["Callvirt", "Call"],
	"cgt"      : ["Cgt", "Cgt_Un"],
	"clt"      : ["Clt", "Clt_Un"],
	"div"      : ["Div", "Div_Un"],
	"ldarg"    : ["Ldarg_S", "Ldarg", "Ldarg_0", "Ldarg_1", "Ldarg_2", "Ldarg_3"],
	"ldarga"   : ["Ldarga_S", "Ldarga"],
	"ldarga"   : ["Ldarga_S", "Ldarga"],
	"ldloc"    : ["Ldloc_S", "Ldloc", "Ldloc_0", "Ldloc_1", "Ldloc_2", "Ldloc_3"],
	"ldloca"   : ["Ldloca_S", "Ldloca"],
	"leave"    : ["Leave", "Leave_S"],
	"mul"      : ["Mul", "Mul_Ovf", "Mul_Ovf_Un"],
	"rem"      : ["Rem", "Rem_Un"],
	"shr"      : ["Shr", "Shr_Un"],
	"starg"    : ["Starg_S", "Starg"],
	"stloc"    : ["Stloc_S", "Stloc", "Stloc_0", "Stloc_1", "Stloc_2", "Stloc_3"],
	"sub"      : ["Sub", "Sub_Ovf", "Sub_Ovf_Un"]
}
def getOpCodes(code):
	stem = code.split(".")[0]
	if code.startswith("conv"):
		print("// note that there is no special casing for conv")	
	if stem in ops:
		return ops[stem]
	else:
		result = code.title()
		result = result.replace(".", "_")
		return [result]
		
vars = []
def handleVars(code):
	stem = code.split(".")[0]	
	if (stem == "stloc" or stem == "ldloc") and code[-2] == "." and code[-1].isupper():
		index = code[-1]
		value = None
		if stem == "stloc":
			value = "((StoreLocal) instruction).Variable"
		elif stem == "ldloc":
			value = "((LoadLocal) instruction).Variable"
			
		if index not in vars:
			vars.append(index)
			print("				int var%s = %s;") % (index, value)
		else:
			print("				if (var%s != %s)") % (index, value)
			print("					break;")
			
def handleCallTarget(code, line):
	if code == "call" or code == "callvirt":
		i = line.find(" ")
		j = line.find(" ", i + 1)
		if j >= 0:
			target = line[j + 1:].strip()
			value = "((Call) instruction).Target.ToString()"

			if target.startswith("*"):
				if target.endswith("*"):
					print("				if (!%s.Contains(\"%s\"))") % (value, target[1:-1])
				else:
					print("				if (!%s.EndsWith(\"%s\"))") % (value, target[1:])
			elif target.endswith("*"):
				print("				if (!%s.StartsWith(\"%s\"))") % (value, target[0:-1])
			elif target.count("*") == 1:
				print("				if (!%s.StartsWith(\"%s\") || !%s.EndsWith(\"%s\"))") % (value, target.split("*")[0], value, target.split("*")[1])
			else:
				print("				if (%s != \"%s\")") % (value, target)
			print("					break;")
	
def processLine(line, i):	
	if i == 0:
		print("				TypedInstruction instruction = m_info.Instructions[index - %s];") % i
	else:
		print("				instruction = m_info.Instructions[index - %s];") % i
	if i == 0:
		print("				Code code = instruction.Untyped.OpCode.Code;")
	else:
		print("				code = instruction.Untyped.OpCode.Code;")

	code = findOpCode(line)
	codes = getOpCodes(code)
	str = "				if ("
	for i in range(len(codes)):
		if i > 0:
			str += " && "
		str += "code != Code."
		str += codes[i]
	str += ")"
	print(str)							# the comma trick doesn't work too well here because each print prepends a space if it doesn't start a new line
	print("					break;")

	handleVars(code)
	handleCallTarget(code, line)
		
	print("")
	
def processLines(lines):
	for i in range(len(lines)): 
		processLine(lines[i], i)
		
def writeProlog(lines):
	print("		public bool DoMatch(int index)")
	print("		{")
	print("			bool match = false;")
	print("")
	print("			do")
	print("			{")
	print("				if (index - %s < 0)") % (len(lines) - 1)
	print("					break;")
	print("")
	
def writeEpilog():
	print("				match = true;")
	print("			}")
	print("			while (false);")
	print("")
	print("			return match;")
	print("		}")
	print("")

# Note that bbedit filters pass a temporary file in as an
# argument instead of using stdin. The fileinput library
# will use a file if it's on the command line or stdin if
# there are no comand line arguments.
lines = []
for candidate in fileinput.input():
	line = candidate.strip()
	if not filterLine(line):
		lines.append(line)
		
for line in lines:
	print("		// %s") % line
lines.reverse()
writeProlog(lines)
processLines(lines)
writeEpilog()