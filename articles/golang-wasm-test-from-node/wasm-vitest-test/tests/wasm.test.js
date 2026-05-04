import { readFileSync } from 'fs';
import { expect, test } from 'vitest'
import * as wasmExec from '../wasm/wasm_exec.js'; // これは必須なので消さないこと。

const go = new Go();
const instancePromise = WebAssembly.instantiate(readFileSync('wasm/main.wasm'), go.importObject).then((result) => {
    const inst = result.instance;
    return go.run(inst).then(() => inst);
});

let instance;

test('calculateSumWasm', async () => {
    instance = await instancePromise;
    
    const iterations = 1000000;
    const sum = instance.exports.calculateSumWasm(iterations);

    expect(sum).toBe(1000000);
});
