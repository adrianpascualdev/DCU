export interface PersonaOption {
    key: string;
    displayName: string;
}

export type LoadPersonas = (signal: AbortSignal) => Promise<readonly PersonaOption[]>;

export async function fetchPersonas(signal: AbortSignal): Promise<readonly PersonaOption[]> {
    const response = await fetch('/api/v1/identity/mock/personas?page=1&pageSize=100', {
        signal,
        headers: { Accept: 'application/json' },
        credentials: 'omit',
    });

    if (!response.ok) {
        throw new Error('No se pudo consultar la lista de personas de prueba.');
    }

    return readPersonaOptions(await response.json());
}

export function readPersonaOptions(payload: unknown): readonly PersonaOption[] {
    if (typeof payload !== 'object' || payload === null || !('items' in payload) || !Array.isArray(payload.items)) {
        throw new Error('La respuesta no contiene una lista de personas de prueba.');
    }

    const items: unknown[] = payload.items;
    return items.map((item) => {
        if (typeof item !== 'object' || item === null
            || !('key' in item) || typeof item.key !== 'string' || !item.key.trim()
            || !('displayName' in item) || typeof item.displayName !== 'string' || !item.displayName.trim()) {
            throw new Error('La respuesta contiene una persona de prueba no válida.');
        }

        return { key: item.key, displayName: item.displayName };
    });
}
