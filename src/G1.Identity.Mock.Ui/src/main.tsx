import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import 'bootstrap/dist/css/bootstrap.min.css';
import './styles/tokens.css';
import { App } from './App';
import { fetchPersonas } from './personas';
import type { LoadPersonas } from './personas';

const loadPersonas: LoadPersonas = import.meta.env.DEV
    ? async () => (await import('./previewPersonas')).previewPersonas
    : fetchPersonas;

const root = document.getElementById('root');
if (!root) {
    throw new Error('No se encontró el contenedor del selector de personas.');
}

createRoot(root).render(
    <StrictMode>
        <App loadPersonas={loadPersonas} preview={import.meta.env.DEV} />
    </StrictMode>,
);
