---
applyTo: "**/*.tsx,**/*.ts,**/*.jsx"
description: "Convenciones de estilo para proyectos React 19+ con TypeScript. Estas instrucciones cubren componentes funcionales, hooks, gestión de estado, rendimiento, accesibilidad y patrones modernos de React."
---

# Directrices de estilo para React y TypeScript

Directrices para escribir aplicaciones React limpias, mantenibles y coherentes con TypeScript, siguiendo la documentación oficial de React en [react.dev](https://react.dev).

## Contexto del proyecto

- **Librería**: React 19+
- **Lenguaje**: TypeScript con modo estricto habilitado
- **Modelo de componentes**: Componentes funcionales con hooks por defecto
- **Referencia de estilo**: [Documentación de React](https://react.dev) y [TypeScript handbook](https://www.typescriptlang.org/docs/)
- **Formato**: El archivo `.editorconfig` siempre tiene precedencia cuando exista

## Principios generales

- Priorizar claridad sobre brevedad: preferir código legible frente a atajos ingeniosos.
- Mantener consistencia: usar formato y nombres uniformes en todo el código.
- Preferir composición sobre herencia: construir UIs complejas componiendo componentes pequeños y centrados.
- Fomentar la seguridad de tipos: usar TypeScript para detectar errores en compilación en lugar de tiempo de ejecución.
- Usar patrones y APIs modernas de React 19+; evitar funcionalidades obsoletas.

## Convenciones de nombres

- Usar `PascalCase` para nombres de componentes, tipos e interfaces.
- Usar `camelCase` para variables, funciones, hooks, props y manejadores de eventos.
- Prefijar hooks personalizados con `use` (por ejemplo, `useAuth`, `useFetchData`).
- Prefijar props de manejadores de eventos con `on` (por ejemplo, `onClick`, `onSubmit`).
- Prefijar implementaciones de manejadores con `handle` (por ejemplo, `handleClick`, `handleSubmit`).
- Usar nombres descriptivos que comuniquen propósito; evitar abreviaturas de una sola letra.
- Nombrar archivos según el componente u hook que exportan (por ejemplo, `UserProfile.tsx`, `useAuth.ts`).

## Estructura del componente

Seguir este orden dentro de un archivo de componente:

```typescript
// 1. Importaciones
import { useState, useEffect } from 'react';
import type { ComponentProps } from './types';

// 2. Tipos e interfaces
interface UserCardProps {
  name: string;
  email: string;
  onSelect: (id: string) => void;
  variant?: 'compact' | 'detailed';
}

// 3. Constantes
const MAX_NAME_LENGTH = 50;

// 4. Definición del componente
export const UserCard: React.FC<UserCardProps> = ({ name, email, onSelect, variant = 'compact' }) => {
  // 5. Hooks (en este orden)
  //    - useState
  //    - useReducer
  //    - useContext
  //    - useRef
  //    - Hooks personalizados
  //    - useEffect

  // 6. Manejadores de eventos
  const handleClick = () => {
    onSelect(email);
  };

  // 7. Funciones auxiliares
  const formatName = (raw: string): string => {
    return raw.length > MAX_NAME_LENGTH ? `${raw.slice(0, MAX_NAME_LENGTH)}…` : raw;
  };

  // 8. Retornos tempranos para estados de carga, error o vacío
  if (!name) return null;

  // 9. Render principal
  return (
    <div onClick={handleClick}>
      <span>{formatName(name)}</span>
      <span>{email}</span>
    </div>
  );
};
```

## Integración con TypeScript

- Definir props con `interface` en la parte superior del archivo.
- Marcar props opcionales con `?` y proporcionar valores por defecto mediante destructuring.
- Usar `React.FC<Props>` para componentes que acepten `children`, o declarar `children: React.ReactNode` explícitamente en la interfaz de props.
- Usar tipos unión para variantes de componente y representaciones de estados finitos.
- Aprovechar tipos incorporados de React como `React.ComponentProps`, `React.MouseEventHandler`, `React.ChangeEvent` y `React.RefObject`.
- Habilitar `strict: true` en `tsconfig.json` para máxima seguridad de tipos.
- Preferir `interface` para props y formas de API pública; usar `type` para uniones, intersecciones y tipos computados.

```typescript
interface ButtonProps {
  label: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary' | 'danger';
  disabled?: boolean;
  icon?: React.ReactNode;
}

export const Button: React.FC<ButtonProps> = ({
  label,
  onClick,
  variant = 'primary',
  disabled = false,
  icon,
}) => {
  return (
    <button type="button" onClick={onClick} disabled={disabled} data-variant={variant}>
      {icon}
      {label}
    </button>
  );
};
```

## Directrices para hooks

### Reglas de hooks

- Llamar hooks únicamente en el nivel superior de un componente o hook personalizado; nunca dentro de bucles, condicionales o funciones anidadas.
- Llamar hooks solo desde componentes funcionales de React o hooks personalizados.
- Incluir siempre todas las dependencias en los arrays de dependencia de `useEffect`, `useMemo` y `useCallback`.

### useState

- Usar nombres descriptivos para variables de estado y sus setters.
- Inicializar el estado con valores por defecto apropiados y tipos explícitos cuando la inferencia sea insuficiente.
- Usar actualizaciones funcionales cuando el nuevo estado dependa del estado anterior.
- Considerar `useReducer` cuando las transiciones de estado sean complejas o interrelacionadas.

```typescript
const [isLoading, setIsLoading] = useState(false);
const [user, setUser] = useState<User | null>(null);

// Actualización funcional
setCount(prevCount => prevCount + 1);
```

### useEffect

- Mantener cada efecto centrado en una sola preocupación.
- Implementar siempre funciones de limpieza para efectos secundarios como suscripciones, timers y listeners.
- Usar hooks `useEffect` separados para efectos no relacionados.
- Usar `AbortController` para peticiones fetch cancelables.

```typescript
useEffect(() => {
  const controller = new AbortController();

  fetchUser(userId, { signal: controller.signal })
    .then(setUser)
    .catch(handleError);

  return () => {
    controller.abort();
  };
}, [userId]);
```

### useMemo y useCallback

- Usar `useMemo` solo para cálculos costosos.
- Usar `useCallback` para estabilizar referencias de funciones pasadas a componentes hijos memoizados.
- No memorizar por defecto; medir con React DevTools Profiler antes de optimizar.
- Incluir siempre todas las dependencias.

```typescript
const sortedItems = useMemo(
  () => items.sort((a, b) => a.price - b.price),
  [items],
);

const handleSubmit = useCallback(
  (data: FormData) => {
    submitForm(data, userId);
  },
  [userId],
);
```

### useRef

- Usar `useRef` para acceder a elementos DOM y almacenar valores mutables que no disparan re-renderizados.
- Tipar refs explícitamente: `useRef<HTMLInputElement>(null)`.

### Hooks personalizados

- Extraer lógica con estado reutilizable en hooks personalizados.
- Devolver objetos con nombres descriptivos cuando haya múltiples valores de retorno.
- Documentar parámetros y valores de retorno con JSDoc cuando el hook se comparta entre módulos.

```typescript
function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const login = async (credentials: Credentials) => {
    const authenticatedUser = await authService.login(credentials);
    setUser(authenticatedUser);
    setIsAuthenticated(true);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    setIsAuthenticated(false);
  };

  return { user, isAuthenticated, login, logout };
}
```

## Novedades de React 19+

### Actions y manejo de formularios

- Usar la prop `action` en elementos `<form>` con funciones async para manejar envíos de formularios.
- Usar `useActionState` para gestionar el estado de la acción del formulario y la bandera `pending`.
- Usar `useFormStatus` de `react-dom` para acceder al estado de envío del formulario padre.

```typescript
import { useActionState } from 'react';
import { useFormStatus } from 'react-dom';

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <button type="submit" disabled={pending}>
      {pending ? 'Enviando…' : 'Enviar'}
    </button>
  );
}

function ContactForm() {
  const [state, formAction] = useActionState(
    async (previousState: FormState, formData: FormData) => {
      const result = await submitContact(formData);
      return result;
    },
    { success: false, message: '' },
  );

  return (
    <form action={formAction}>
      <input type="text" name="name" required />
      <SubmitButton />
      {state.message && <p>{state.message}</p>}
    </form>
  );
}
```

### useOptimistic

- Usar `useOptimistic` para mostrar retroalimentación UI inmediata mientras una mutación async está en curso.
- El estado optimista revierte automáticamente si la acción falla.

```typescript
import { useOptimistic } from 'react';

function ChangeName({ currentName, onUpdateName }: ChangeNameProps) {
  const [optimisticName, setOptimisticName] = useOptimistic(currentName);

  const submitAction = async (formData: FormData) => {
    const newName = formData.get('name') as string;
    setOptimisticName(newName);
    const updatedName = await updateName(newName);
    onUpdateName(updatedName);
  };

  return (
    <form action={submitAction}>
      <p>Tu nombre es: {optimisticName}</p>
      <input type="text" name="name" disabled={currentName !== optimisticName} />
    </form>
  );
}
```

### Hook `use`

- Usar el hook `use` para leer promesas y valores de contexto dentro de componentes.
- Envolver el componente consumidor con `<Suspense>` para manejar el estado pendiente de promesas.

```typescript
import { use } from 'react';

function Message({ messagePromise }: { messagePromise: Promise<string> }) {
  const messageContent = use(messagePromise);
  return <p>{messageContent}</p>;
}
```

### `ref` como prop

- En React 19+, los componentes funcionales aceptan `ref` directamente como prop sin `forwardRef`.
- `forwardRef` está obsoleto y se eliminará en una versión futura.

```typescript
function MyInput({ placeholder, ref }: { placeholder: string; ref: React.Ref<HTMLInputElement> }) {
  return <input placeholder={placeholder} ref={ref} />;
}
```

### Funciones de limpieza de refs

- Las callbacks de ref pueden devolver una función de limpieza que React llama cuando la ref se desapega.

```typescript
<div
  ref={(node) => {
    if (node) {
      node.focus();
    }
    return () => {
      // Limpieza cuando la ref se desapega
    };
  }}
/>
```

### Metadatos del documento

- Renderizar `<title>`, `<meta>` y `<link>` en cualquier punto del árbol de componentes; React los eleva automáticamente al `<head>`.

### Precarga de recursos

- Usar `preinit`, `preload`, `preconnect` y `prefetchDNS` de `react-dom` para señalar necesidades de recursos al navegador anticipadamente.

```typescript
import { prefetchDNS, preconnect, preload, preinit } from 'react-dom';

function MyComponent() {
  preinit('https://cdn.example.com/script.js', { as: 'script' });
  preload('https://cdn.example.com/font.woff2', { as: 'font' });
  preconnect('https://api.example.com');
}
```

## Gestión de estado

- Usar `useState` para estado local sencillo.
- Usar `useReducer` para lógica de estado compleja con múltiples subvalores o transiciones.
- Usar `useContext` para compartir estado en un subárbol sin prop-drilling.
- Evitar pasar props por más de dos o tres niveles; usar Context o composición en su lugar.
- Considerar librerías externas de gestión de estado (Redux Toolkit, Zustand) cuando el estado sea verdaderamente global y complejo.
- Usar librerías de fetch de datos (TanStack Query, SWR) para estado servidor con caching, refetch y actualizaciones en segundo plano.

## Buenas prácticas de JSX

### Render condicional

- Usar `&&` para renders condicionales simples.
- Usar ternarios para condicionales de dos ramas.
- Evitar ternarios anidados profundos; extraer en funciones auxiliares o componentes separados.

```typescript
{isLoading && <Spinner />}
{error ? <ErrorMessage error={error} /> : <Content data={data} />}
{items.length > 0 ? <ItemList items={items} /> : <EmptyState />}
```

### Renderizado de listas

- Proveer siempre `key` únicas y estables derivadas de identificadores de datos.
- No usar el índice del array como `key` en listas dinámicas.

```typescript
{users.map(user => (
  <UserCard key={user.id} user={user} />
))}
```

### Manejadores de eventos

- Definir funciones manejadoras fuera del JSX para legibilidad y testabilidad.
- Usar funciones flecha en JSX solo al pasar argumentos.
- Evitar lógica inline multilínea en atributos de evento JSX.

```typescript
const handleDeleteUser = (userId: string) => {
  deleteUser(userId);
};

return (
  <button type="button" onClick={() => handleDeleteUser(user.id)}>
    Eliminar
  </button>
);
```

## Patrones de composición de componentes

### Separación container/presentación

- Los componentes presentacionales reciben datos y callbacks vía props y renderizan UI.
- Los componentes container gestionan estado, efectos y fetch de datos.

```typescript
// Presentacional
interface UserListProps {
  users: User[];
  onSelectUser: (user: User) => void;
}

export const UserList: React.FC<UserListProps> = ({ users, onSelectUser }) => (
  <ul>
    {users.map(user => (
      <li key={user.id} onClick={() => onSelectUser(user)}>
        {user.name}
      </li>
    ))}
  </ul>
);

// Contenedor
export const UserListContainer: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [, setSelectedUser] = useState<User | null>(null);

  useEffect(() => {
    fetchUsers().then(setUsers);
  }, []);

  return <UserList users={users} onSelectUser={setSelectedUser} />;
};
```

### Componentes compuestos

- Usar componentes compuestos para elementos UI estrechamente relacionados que comparten estado implícito.

```typescript
<Tabs>
  <TabList>
    <Tab>Perfil</Tab>
    <Tab>Configuración</Tab>
  </TabList>
  <TabPanels>
    <TabPanel>Contenido de Perfil</TabPanel>
    <TabPanel>Contenido de Configuración</TabPanel>
  </TabPanels>
</Tabs>
```

### Otros patrones

- HOCs: envolver componentes para inyectar preocupaciones transversales (autenticación, logging).
- Render props: compartir lógica con estado pasando una función de render como child o prop.
- Provider pattern: combinar `useContext` con un componente Provider para distribuir estado compartido.

## Optimización de rendimiento

### React.memo

- Usar `React.memo` para componentes que rendericen con frecuencia con las mismas props.
- Proveer función de comparación personalizada cuando la comparación superficial sea insuficiente.
- Medir con React DevTools Profiler antes de memoizar.

```typescript
export const ExpensiveComponent = React.memo<ExpensiveComponentProps>(
  ({ data, onAction }) => {
    // Render logic
    return <div>{data.label}</div>;
  },
  (prevProps, nextProps) => prevProps.data.id === nextProps.data.id,
);
```

### Code splitting

- Usar `React.lazy` para división de código a nivel de rutas.
- Envolver componentes lazy con `<Suspense>` y proporcionar un fallback significativo.
- Dividir en los límites de ruta, no a nivel de componente individual salvo necesidad.

```typescript
import { lazy, Suspense } from 'react';

const Dashboard = lazy(() => import('./pages/Dashboard'));
const Profile = lazy(() => import('./pages/Profile'));

function App() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <Routes>
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/profile" element={<Profile />} />
      </Routes>
    </Suspense>
  );
}
```

### Guía de rendimiento general

- Evitar crear nuevos objetos o arrays dentro del render salvo que estén memoizados.
- Sacar cálculos costosos del path de render con `useMemo`.
- Usar bibliotecas de virtual scrolling para listas grandes.
- Perfilar componentes con React DevTools antes de aplicar optimizaciones.

## Manejo de errores

### Error boundaries

- Implementar error boundaries alrededor de secciones críticas de UI para evitar fallos de página completos.
- Proporcionar una UI de fallback y enviar errores a un servicio de monitorización.
- Las error boundaries deben ser componentes de clase, ya que no existe equivalente por hook.

```typescript
import { Component, ErrorInfo, ReactNode } from 'react';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback ?? <p>Se ha producido un error.</p>;
    }
    return this.props.children;
  }
}
```

### Manejo asíncrono de errores

- Manejar errores en `useEffect` y en manejadores de eventos con try-catch.
- Mostrar estados de error significativos a los usuarios.
- Usar estados de error en hooks de fetch para condicionar la UI.

## Formularios y validación

- Usar componentes controlados para inputs de formularios.
- Usar librerías como React Hook Form o Formik para escenarios de formularios complejos.
- Implementar formularios accesibles con `<label>`, `aria-describedby` y atributos `htmlFor` correctos.
- Usar validación con debounce para campos que requieran comprobaciones costosas.

## Routing

- Usar React Router (o el router del framework) para navegación del lado cliente.
- Implementar code splitting por rutas con `React.lazy`.
- Proteger rutas con guards de autenticación.
- Manejar parámetros de ruta y query strings con wrappers tipados.

## Testing

- Escribir tests con React Testing Library y un runner como Jest o Vitest.
- Probar comportamiento de componentes e interacciones de usuario, no detalles de implementación.
- Usar consultas `screen` (`getByRole`, `getByLabelText`, `getByText`) en lugar de `getByTestId`.
- Mockear dependencias externas y llamadas API.
- Probar accesibilidad assertando atributos ARIA y navegación por teclado.

```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { Button } from './Button';

describe('Button', () => {
  it('calls onClick when clicked', () => {
    const handleClick = jest.fn();
    render(<Button label="Click me" onClick={handleClick} />);

    fireEvent.click(screen.getByRole('button', { name: 'Click me' }));

    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});
```

## Seguridad

- No pasar nunca entrada de usuario sin sanear a `dangerouslySetInnerHTML`.
- Sanear datos antes de renderizar contenido dinámico.
- Usar HTTPS para toda comunicación externa con APIs.
- Implementar patrones adecuados de autenticación y autorización.
- Evitar almacenar datos sensibles en `localStorage` o `sessionStorage`.
- Aplicar cabeceras de Content Security Policy (CSP).

## Accesibilidad

- Usar elementos HTML semánticos (`<button>`, `<nav>`, `<main>`, `<header>`, `<footer>`).
- Implementar atributos y roles ARIA cuando el HTML semántico sea insuficiente.
- Asegurar que todos los elementos interactivos sean accesibles por teclado.
- Proveer texto alternativo para imágenes y etiquetas descriptivas para iconos.
- Mantener ratios de contraste de color adecuados (mínimo WCAG AA).
- Probar con lectores de pantalla y herramientas de accesibilidad del navegador.
