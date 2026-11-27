import { readPersonaOptions } from './personas';

// Ejemplo de listMockPersonas en openapi.v1.yaml, solo para el servidor de Vite.
export const previewPersonas = readPersonaOptions({
    fixtureVersion: '1.0.0',
    items: [{
        key: 'coach.ana',
        userId: 'usr_01HXCOACHANA',
        displayName: 'Ana Entrenadora',
        email: 'ana.entrenadora@example.test',
        emailVerified: true,
        phoneVerified: true,
        status: 'active',
        isMinor: false,
        roles: ['coach'],
        permissions: [],
        authenticationMethods: ['mock_persona'],
        labels: ['restricted-scope'],
    }],
    page: 1,
    pageSize: 20,
    total: 1,
});
