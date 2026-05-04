package main

//go:wasmexport calculateSumWasm
func calculateSumWasm(iterations int32) int32 {
	var sum int32 = 0
	for i := int32(0); i < iterations; i++ {
		sum++
	}
	return sum
}

func main() {
}
