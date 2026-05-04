declare class Go {
    constructor();
    importObject: WebAssembly.Imports;
    run(instance: WebAssembly.Instance): Promise<void>;
    // 必要に応じて他のメソッドやプロパティの型定義を追加
    // (exit など)
}