import { useEffect, useRef, useState } from 'react';
import { PersonaSelector } from './components/PersonaSelector';
import type { LoadPersonas, PersonaOption } from './personas';
import styles from './styles/selector.module.css';

type PersonaListState =
    | { status: 'loading' }
    | { status: 'error' }
    | { status: 'loaded'; personas: readonly PersonaOption[] };

interface AppProps {
    loadPersonas: LoadPersonas;
    preview?: boolean;
    onContinueAuthorization?: (personaKey: string) => void | Promise<void>;
}

export function App({ loadPersonas, preview = false, onContinueAuthorization }: AppProps) {
    const [list, setList] = useState<PersonaListState>({ status: 'loading' });
    const [attempt, setAttempt] = useState(0);
    const headingRef = useRef<HTMLHeadingElement>(null);

    useEffect(() => {
        const controller = new AbortController();

        async function load() {
            try {
                const personas = await loadPersonas(controller.signal);
                if (!controller.signal.aborted) {
                    setList({ status: 'loaded', personas });
                }
            } catch {
                if (!controller.signal.aborted) {
                    setList({ status: 'error' });
                }
            }
        }

        void load();
        return () => controller.abort();
    }, [loadPersonas, attempt]);

    function handleRetry() {
        headingRef.current?.focus();
        setList({ status: 'loading' });
        setAttempt((previous) => previous + 1);
    }

    const isEmpty = list.status === 'loaded' && list.personas.length === 0;

    return (
        <main className={`container-fluid ${styles.page}`}>
            <div className="row justify-content-center">
                <div className="col-12 col-md-8 col-lg-6">
                    <section className={`card ${styles.panel}`} aria-labelledby="selector-title">
                        <div className={`card-body ${styles.content}`}>
                            <header>
                                <h1 id="selector-title" ref={headingRef} tabIndex={-1}>Selector de personas de prueba</h1>
                                <p className={`alert alert-warning ${styles.notice}`}>
                                    Los datos son ficticios y se pierden al reiniciar el servicio.
                                    No introduzcas datos reales.
                                </p>
                            </header>

                            {preview && <p>Vista de desarrollo: puedes probar la selección sin iniciar sesión.</p>}

                            <div role="status">
                                {list.status === 'loading' && <p>Cargando personas de prueba…</p>}
                                {isEmpty && <p>No hay personas de prueba disponibles.</p>}
                            </div>
                            <div role="alert">
                                {list.status === 'error' && (
                                    <p className={`alert alert-danger ${styles.error}`}>
                                        No se pudo cargar la lista de personas de prueba. Vuelve a intentarlo.
                                    </p>
                                )}
                            </div>

                            {(isEmpty || list.status === 'error') && (
                                <button className={`btn btn-secondary ${styles.secondaryButton}`} type="button" onClick={handleRetry}>
                                    Volver a cargar la lista
                                </button>
                            )}
                            {list.status === 'loaded' && !isEmpty && (
                                <PersonaSelector
                                    personas={list.personas}
                                    preview={preview}
                                    onContinueAuthorization={onContinueAuthorization}
                                />
                            )}
                        </div>
                    </section>
                </div>
            </div>
        </main>
    );
}
